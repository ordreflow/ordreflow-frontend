// Draws a horizontal bar chart. `items` is already ranked by the backend.
export function render(container, items) {
    container.replaceChildren();
    if (!window.d3 || !items || items.length === 0) return;

    const d3 = window.d3;
    const rowHeight = 36;
    const margin = { top: 8, right: 56, bottom: 28, left: 170 };
    const width = Math.max(container.clientWidth, 320);
    const height = margin.top + margin.bottom + rowHeight * items.length;

    const svg = d3.select(container)
        .append("svg")
        .attr("viewBox", `0 0 ${width} ${height}`)
        .attr("width", "100%")
        .attr("role", "img")
        .attr("aria-label", "Top ordrer efter registrerede timer");

    const x = d3.scaleLinear()
        .domain([0, d3.max(items, d => d.totalHours) || 1])
        .nice()
        .range([margin.left, width - margin.right]);

    const y = d3.scaleBand()
        .domain(items.map(d => d.orderId))
        .range([margin.top, height - margin.bottom])
        .padding(0.25);

    const truncate = name => name.length > 22 ? `${name.slice(0, 21)}…` : name;
    const nameById = new Map(items.map(d => [d.orderId, d.orderName]));

    svg.append("g")
        .attr("transform", `translate(0,${height - margin.bottom})`)
        .call(d3.axisBottom(x).ticks(5))
        .call(g => g.append("text")
            .attr("x", width - margin.right)
            .attr("y", 24)
            .attr("fill", "currentColor")
            .attr("text-anchor", "end")
            .text("Timer"));

    svg.append("g")
        .attr("transform", `translate(${margin.left},0)`)
        .call(d3.axisLeft(y).tickFormat(id => truncate(nameById.get(id))).tickSizeOuter(0));

    const tooltip = d3.select(container)
        .append("div")
        .attr("class", "top-orders-tooltip")
        .style("display", "none");

    svg.append("g")
        .selectAll("rect")
        .data(items)
        .join("rect")
        .attr("x", x(0))
        .attr("y", d => y(d.orderId))
        .attr("width", d => x(d.totalHours) - x(0))
        .attr("height", y.bandwidth())
        .attr("rx", 4)
        .attr("fill", "#2f6fdb")
        .on("mousemove", (event, d) => {
            const [px, py] = d3.pointer(event, container);
            tooltip
                .style("display", "block")
                .style("left", `${px + 12}px`)
                .style("top", `${py + 12}px`)
                .text(`${d.orderName}: ${d.totalHours.toLocaleString("da-DK")} timer`);
        })
        .on("mouseleave", () => tooltip.style("display", "none"));

    svg.append("g")
        .selectAll("text")
        .data(items)
        .join("text")
        .attr("x", d => x(d.totalHours) + 6)
        .attr("y", d => y(d.orderId) + y.bandwidth() / 2)
        .attr("dy", "0.35em")
        .attr("font-size", 12)
        .attr("fill", "currentColor")
        .text(d => d.totalHours.toLocaleString("da-DK"));
}
