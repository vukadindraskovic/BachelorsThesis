let live = true;

class Jam {
    constructor(time, lane, maxVehicleSpeed, timestep, vehicleCount) {
        this.time = time;
        this.lane = lane;
        this.maxVehicleSpeed = maxVehicleSpeed;
        this.timestep = timestep;
        this.vehicleCount = vehicleCount;
    }
}

const connection = new signalR.HubConnectionBuilder()
    .withUrl("https://localhost:9000/TrafficJamHub")
    .build();

async function start() {
    try {
        await connection.start();
        console.log("SignalR Connected.");
    } catch (err) {
        console.log(err);
        setTimeout(start, 5000);
    }
};

connection.on("onNewTrafficJam", (jam, time) => {
    if (!live) return;
    const obj = JSON.parse(jam);
    fillTableWithJam(new Jam(time, obj.lane, obj.max_vehicle_speed, obj.timestep, obj.vehicle_count));
})

start();

const getButton = document.querySelector(".getButtonClass");
const liveButton = document.querySelector(".liveButtonClass");

getButton.disabled = true;
getButton.addEventListener("click", onGetHandler);
liveButton.addEventListener("click", onGoLiveHandler)

function onGoLiveHandler() {
    live = !live;
    liveButton.innerHTML = live === true ? "Stop" : "Go Live";
    getButton.disabled = live
    document.querySelectorAll(".trClass").forEach(e => e.remove());
}

async function onGetHandler() {
    if (live) return;
    const lane = document.querySelector("input[type='text']").value
    let url = "http://localhost:80/TrafficJam";
    if (lane != "") {
        url += "?lane=" + encodeURIComponent(lane)
    }
    document.querySelectorAll(".trClass").forEach(e => e.remove());

    const response = await fetch(url)
    const jams = await response.json()
    jams.forEach(j => fillTableWithJam(j))
}


function fillTableWithJam(jam) {
    const table = document.querySelector(".tableClass")

    let tr = document.createElement("tr");
    tr.className = "trClass"

    let tdTime = document.createElement("td");
    let tdLane = document.createElement("td");
    let tdTimestep = document.createElement("td");
    let tdMaxVehicleSpeed = document.createElement("td");
    let tdVehicleCount = document.createElement("td");

    tdTime.innerHTML = jam.time;
    tdLane.innerHTML = jam.lane;
    tdTimestep.innerHTML = jam.timestep;
    tdMaxVehicleSpeed.innerHTML = jam.maxVehicleSpeed;
    tdVehicleCount.innerHTML = jam.vehicleCount;

    tr.appendChild(tdTime);
    tr.appendChild(tdLane);
    tr.appendChild(tdTimestep);
    tr.appendChild(tdMaxVehicleSpeed);
    tr.appendChild(tdVehicleCount);

    table.appendChild(tr)
}