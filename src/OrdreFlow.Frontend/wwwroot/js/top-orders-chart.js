
export function render(container, orders, dotNetReference) {
    const d3 = window.d3;

    if (!d3) {
        console.error("D3.js is not loaded.");
        return;
    }

    container.replaceChildren();

    if (!orders || orders.length === 0) {
        return;
    }

    const margin = {
        top: 25,
        right: 75,
        bottom: 45,
        left: 125
    };

    const width = Math.max(container.clientWidth, 350);
    const barHeight = 34;
    const rowGap = 18;

    const chartBottom =
        margin.top + orders.length * (barHeight + rowGap);

    const height = chartBottom + margin.bottom;

    // Collect all unique tasks.
    const taskMap = new Map();

    orders.forEach(order => {
        (order.tasks ?? []).forEach(task => {
            taskMap.set(task.taskId, {
                id: task.taskId,
                name: task.taskName
            });
        });
    });

    const tasks = Array.from(taskMap.values());

    // Sort tasks by ID to keep the color assignment consistent.
    tasks.sort((a, b) =>
        String(a.id).localeCompare(String(b.id))
    );

    const taskKeys = tasks.map(task => task.id);

    // Create one data row for each order.
    const data = orders.map(order => {
        const row = {
            orderId: order.orderId,
            orderName: order.orderName,
            totalHours: order.totalHours
        };

        taskKeys.forEach(taskId => {
            row[taskId] = 0;
        });

        (order.tasks ?? []).forEach(task => {
            row[task.taskId] = task.totalHours;
        });

        return row;
    });

    // Consistent task colors.
    const palette = d3.schemeTableau10.concat(d3.schemeSet3);

    const color = d3.scaleOrdinal()
        .domain(taskKeys)
        .range(palette);

    const maxHours = d3.max(data, d => d.totalHours) || 1;

    const x = d3.scaleLinear()
        .domain([0, maxHours])
        .nice()
        .range([margin.left, width - margin.right]);

    const y = d3.scaleBand()
        .domain(data.map(d => d.orderId))
        .range([
            margin.top,
            chartBottom
        ])
        .padding(0.25);

    const svg = d3.select(container)
        .append("svg")
        .attr("viewBox", `0 0 ${width} ${height}`)
        .attr("width", "100%")
        .attr("height", "auto");

    // X-axis.
    svg.append("g")
        .attr("transform", `translate(0, ${chartBottom})`)
        .call(d3.axisBottom(x).ticks(6))
        .call(g => g.select(".domain").attr("stroke", "currentColor"));

    // Y-axis with order names.
    svg.append("g")
        .attr("transform", `translate(${margin.left}, 0)`)
        .call(
            d3.axisLeft(y)
                .tickFormat(id =>
                    data.find(d => d.orderId === id)?.orderName ?? ""
                )
        )
        .call(g => g.select(".domain").remove());

    // Tooltip.
    const tooltip = d3.select(container)
        .append("div")
        .style("position", "absolute")
        .style("visibility", "hidden")
        .style("background", "#1f2937")
        .style("color", "#ffffff")
        .style("padding", "10px 12px")
        .style("border-radius", "6px")
        .style("font-size", "13px")
        .style("pointer-events", "none")
        .style("z-index", "10")
        .style("line-height", "1.5");

    // Create stacked bars.
    const stack = d3.stack()
        .keys(taskKeys)
        .value((row, key) => row[key]);

    const series = stack(data);

    svg.selectAll(".task-series")
        .data(series)
        .join("g")
        .attr("class", "task-series")
        .attr("fill", d => color(d.key))
        .selectAll("rect")
        .data(seriesItem =>
            seriesItem.map(segment => ({
                segment,
                taskId: seriesItem.key,
                order: segment.data
            }))
        )
        .join("rect")
        .attr("x", d => x(d.segment[0]))
        .attr("y", d => y(d.order.orderId))
        .attr("width", d =>
            Math.max(0, x(d.segment[1]) - x(d.segment[0]))
        )
        .attr("height", y.bandwidth())
        .attr("rx", 2)
        .style("cursor", "pointer")

        // Tooltip on hover.
        .on("mousemove", function (event, d) {
            const task = taskMap.get(d.taskId);
            const hours = d.order[d.taskId];

            const percentage = d.order.totalHours > 0
                ? (hours / d.order.totalHours) * 100
                : 0;

            tooltip
                .style("visibility", "visible")
                .html(`
                    <strong>${escapeHtml(d.order.orderName)}</strong><br>
                    Opgave: ${escapeHtml(task?.name ?? "Ukendt")}<br>
                    Timer: ${hours.toLocaleString("da-DK")}<br>
                    Andel: ${percentage.toFixed(1).replace(".", ",")}%<br>
                    I alt på ordre:
                    ${d.order.totalHours.toLocaleString("da-DK")} timer
                `);

            const bounds = container.getBoundingClientRect();

            tooltip
                .style("left", `${event.clientX - bounds.left + 12}px`)
                .style("top", `${event.clientY - bounds.top + 12}px`);
        })

        .on("mouseleave", () => {
            tooltip.style("visibility", "hidden");
        })

        // Select order on click.
        .on("click", function (event, d) {
            if (dotNetReference) {
                dotNetReference.invokeMethodAsync(
                    "SelectOrder",
                    d.order.orderId
                );
            }
        });

    // Total hours label at the end of each bar.
    svg.selectAll(".total-label")
        .data(data)
        .join("text")
        .attr("class", "total-label")
        .attr("x", d => x(d.totalHours) + 6)
        .attr("y", d => y(d.orderId) + y.bandwidth() / 2)
        .attr("dy", "0.35em")
        .attr("font-size", "12px")
        .attr("fill", "currentColor")
        .text(d => `${d.totalHours.toLocaleString("da-DK")} t`);
}

function escapeHtml(value) {
    return String(value).replace(/[&<>"']/g, character => ({
        "&": "&amp;",
        "<": "&lt;",
        ">": "&gt;",
        '"': "&quot;",
        "'": "&#39;"
    })[character]);
}
