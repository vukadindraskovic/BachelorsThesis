Bachelor's thesis

To start the app, go to root folder and type "docker compose up". If you made any changes to some of the files, add --build flag.
Python script that simulates events in the traffic is located in vehicle-simulator folder and you can run it by 'py vehicles.py' from the vehicle-simulator folder.
Note that you have to wait for RabbitMQ from docker compose to set up before you run the simulator.

Ekuiper uses stream called 'vehicle_stream'.
Rule used for Ekuiper is called 'traffic jam control' and its SQL body is:
    'select timestep_time as timestep, vehicle_lane as lane, max(vehicle_speed) as max_vehicle_speed, count(*) as vehicle_count
    from vehicle_stream
    group by timestep_time, vehicle_lane, tumblingwindow(ss, 2)
    having max(vehicle_speed) < 20 AND count(*) >= 2'.
The detected jam is sent to 'ekuiper/jam' topic.

There is Web client used for showing the traffic jam. There is real time notification option and query option for results.
Real time notification works with Monitoring service through SignalR, while static querying works with RestAPI through HTTP GET requests.

For InfluxDb there is organization called 'vukadin' and bucket called 'bachelorsThesis'.
You will get new token different from mine from InfluxDb when you create account, so you should change it in the monitoring service.
