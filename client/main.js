let queryResultCount = 0;

const getButton = document.querySelector(".getButtonClass");
const clearButton = document.querySelector(".clearButtonClass");

getButton.addEventListener("click", onGetHandler);
clearButton.addEventListener("click", onClearHandler)

function onClearHandler() {
    const input = document.querySelector("input[type='text']")
    input.value = ""
}

async function onGetHandler() {
    const lane = document.querySelector("input[type='text']").value
    let url = "http://localhost:80/TrafficJam";
    if (lane != "") {
        url += "?lane=" + encodeURIComponent(lane)
    }

    const response = await fetch(url)
    const jams = await response.json()
    const table = document.querySelector(".tableClass")

    table.querySelectorAll(".trClass").forEach(e => e.remove());

    jams.forEach(element => {
        let tr = document.createElement("tr");
        tr.className = "trClass"
    
        let tdTime = document.createElement("td");
        let tdLane = document.createElement("td");
        let tdTimestep = document.createElement("td");
        let tdMaxVehicleSpeed = document.createElement("td");
        let tdVehicleCount = document.createElement("td");
    
        tdTime.innerHTML = element.time;
        tdLane.innerHTML = element.lane;
        tdTimestep.innerHTML = element.timestep;
        tdMaxVehicleSpeed.innerHTML = element.maxVehicleSpeed;
        tdVehicleCount.innerHTML = element.vehicleCount;
    
        tr.appendChild(tdTime);
        tr.appendChild(tdLane);
        tr.appendChild(tdTimestep);
        tr.appendChild(tdMaxVehicleSpeed);
        tr.appendChild(tdVehicleCount);
    
        table.appendChild(tr)
    });
}