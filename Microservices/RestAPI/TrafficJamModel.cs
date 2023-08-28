using InfluxDB.Client.Core;

namespace RestAPI;

[Measurement("traffic_jam")]
public class TrafficJamModel
{
    [Column(IsTimestamp = true)]
    public DateTime Time { get; set; }

    [Column("lane", IsTag = true)]
    public string Lane { get; set; }

    [Column("timestep")]
    public double Timestep { get; set; }

    [Column("max_vehicle_speed")]
    public double MaxVehicleSpeed { get; set; }

    [Column("vehicle_count", IsTag = true)]
    public int VehicleCount { get; set; }
}
