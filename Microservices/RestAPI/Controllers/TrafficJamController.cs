using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using Microsoft.AspNetCore.Mvc;

namespace RestAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TrafficJamController : ControllerBase
    {
        private readonly InfluxDBClient _idbClient; 
        private readonly string influxdbOrganization = "vukadin";
        private readonly string influxdbBucket = "bachelorsThesis";

        public TrafficJamController(InfluxDBClient idbClient)
        {
            _idbClient = idbClient;
        }

        [HttpGet]
        public async Task<ActionResult> GetLane([FromQuery] string lane = "")
        {
            var iqlQuery = $"from(bucket: \"{influxdbBucket}\") " +
                            "|> range(start: 0) " +
                            "|> filter(fn: (r) => r[\"_field\"] == \"timestep\" or r[\"_field\"] == \"max_vehicle_speed\") ";

            if (lane != "") iqlQuery += $"|> filter(fn: (r) => r[\"lane\"] == \"{lane}\") ";

            iqlQuery += "|> pivot(rowKey: [\"_time\", \"lane\"], columnKey: [\"_field\"], valueColumn: \"_value\")";
            var result = await _idbClient.GetQueryApi().QueryAsync<TrafficJamModel>(iqlQuery, influxdbOrganization);
            return Ok(result.OrderBy(r => r.Time));
        }
    }
}