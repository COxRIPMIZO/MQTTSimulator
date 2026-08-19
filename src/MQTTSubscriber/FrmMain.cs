using MQTTnet;
using System.Text;
using System.Text.Json;

namespace MQTTSubscriber
{
    public partial class FrmMain : Form
    {
        private IMqttClient _mqttclient;
        public FrmMain()
        {
            InitializeComponent();
        }
        private async void FrmMain_Load(object sender, EventArgs e)
        {
            var mqttFactory = new MqttClientFactory();

            _mqttclient = mqttFactory.CreateMqttClient();

            //options
            var mqttOptions = new MqttClientOptionsBuilder()
                //specify the server and port to connect to
                .WithTcpServer("localhost", 1883)
                //UNIQUE BTO EACH CLIENT EVEN FOR SUSBSCRIBERS
                .WithClientId("TestSubscriber")

                .Build();

            //check for connection
            var res = await _mqttclient.ConnectAsync(mqttOptions, CancellationToken.None);

            if (res.ResultCode == MqttClientConnectResultCode.Success)
            {
                MessageBox.Show("Connected to MQTT broker successfully!", "Connection Status", MessageBoxButtons.OK, MessageBoxIcon.Information);

                //subscribe to topic
                _mqttclient.ApplicationMessageReceivedAsync += ApplicationMessageReceivedAsync;

                string status = "sensor/status";

                var mqttSubscribeOptions = new MqttClientSubscribeOptionsBuilder()
                    .WithTopicFilter("sensor/1/data")
                    .WithTopicFilter("sensor/2/data")
                    .WithTopicFilter("sensor/3/data")
                    .WithTopicFilter("sensor/4/data")
                    .WithTopicFilter(status)
                    .Build();

                var subscriptionResult = await _mqttclient.SubscribeAsync(mqttSubscribeOptions, CancellationToken.None);
            }
            else
            {
                MessageBox.Show($"Failed to connect to MQTT broker. Result code: {res.ResultCode}", "Connection Status", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Task ApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs args)
        {
            // Extract payload data and topic name safely
            var data = args.ApplicationMessage.ConvertPayloadToString();
            var incomingTopic = args.ApplicationMessage.Topic;

            // Define your topics
            string statusTopic = "sensor/status";
            string dataTopic = "sensor";

            if (incomingTopic.Equals(statusTopic, StringComparison.OrdinalIgnoreCase))
            {
                if (lblStatus.InvokeRequired)
                {
                    lblStatus.Invoke(new MethodInvoker(() => UpdateStatusLabel(data)));
                }
                else
                {
                    UpdateStatusLabel(data);
                }
            }
            else if (incomingTopic.Equals("sensor/1/data", StringComparison.OrdinalIgnoreCase))
            {
                var jsonData = JsonSerializer.Deserialize<PyloadData>(data);

                label1.Invoke(new MethodInvoker(() => label1.Text = jsonData?.Value.ToString()));
            }
            else if (incomingTopic.Equals("sensor/2/data", StringComparison.OrdinalIgnoreCase))
            {
                var jsonData = JsonSerializer.Deserialize<PyloadData>(data);

                label2.Invoke(new MethodInvoker(() => label2.Text = jsonData?.Value.ToString()));
            }
            else if (incomingTopic.Equals("sensor/3/data", StringComparison.OrdinalIgnoreCase))
            {
                var jsonData = JsonSerializer.Deserialize<PyloadData>(data);

                label3.Invoke(new MethodInvoker(() => label3.Text = jsonData?.Value.ToString()));
            }
            else if (incomingTopic.Equals("sensor/4/data", StringComparison.OrdinalIgnoreCase))
            {
                var jsonData = JsonSerializer.Deserialize<PyloadData>(data);

                label4.Invoke(new MethodInvoker(() => label4.Text = jsonData?.Value.ToString()));
            }

            return Task.CompletedTask;
        }

        private void UpdateStatusLabel(string statusMessage)
        {
            string cleanStatus = statusMessage.Trim().ToLower();

            if (cleanStatus == "online" || cleanStatus == "connected" || cleanStatus == "true")
            {
                lblStatus.Text = "Connected";
                lblStatus.ForeColor = Color.Green;
            }
            else
            {
                lblStatus.Text = "Disconnected";
                lblStatus.ForeColor = Color.Red;
            }
        }

    }

    public class PyloadData
    {
        public string? Name { get; set; }
        public int Value { get; set; }
        public string? TimeStamp { get; set; }
    }
}
