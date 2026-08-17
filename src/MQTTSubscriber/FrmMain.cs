using MQTTnet;
using System.Text;

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

                //string[] topic = { "sensor/1/data", "sensor/2/data", "sensor/3/data", "sensor/4/data" };
                string status = "sensor/status";

                //create susbscription options for topics to listen
                var mqttSubscribeOptions = new MqttClientSubscribeOptionsBuilder()
                    .WithTopicFilter("sensor/1/data")
                    .WithTopicFilter("sensor/2/data")
                    .WithTopicFilter("sensor/3/data")
                    .WithTopicFilter("sensor/4/data")
                    .WithTopicFilter(status)
                    .Build();

                //subscribe to the topic
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

            // Scenario A: The message belongs to the status topic
            if (incomingTopic.Equals(statusTopic, StringComparison.OrdinalIgnoreCase))
            {
                // Safe UI dispatch for the Label
                if (lblStatus.InvokeRequired)
                {
                    lblStatus.Invoke(new MethodInvoker(() => UpdateStatusLabel(data)));
                }
                else
                {
                    UpdateStatusLabel(data);
                }
            }
            // Scenario B: The message belongs to your data topic
            else if (incomingTopic.Equals("sensor/1/data", StringComparison.OrdinalIgnoreCase))
            {
                label1.Invoke(new MethodInvoker(() => label1.Text = data));
            }
            else if (incomingTopic.Equals("sensor/2/data", StringComparison.OrdinalIgnoreCase))
            {
                label2.Invoke(new MethodInvoker(() => label2.Text = data));
            }
            else if (incomingTopic.Equals("sensor/3/data", StringComparison.OrdinalIgnoreCase))
            {
                label3.Invoke(new MethodInvoker(() => label3.Text = data));
            }
            else if (incomingTopic.Equals("sensor/4/data", StringComparison.OrdinalIgnoreCase))
            {
                label4.Invoke(new MethodInvoker(() => label4.Text = data));
            }

            return Task.CompletedTask;
        }

        // Separate helper method to update the UI label safely
        private void UpdateStatusLabel(string statusMessage)
        {
            // Clean up string spaces and make it case-insensitive
            string cleanStatus = statusMessage.Trim().ToLower();

            if (cleanStatus == "online" || cleanStatus == "connected" || cleanStatus == "true")
            {
                lblStatus.Text = "● Connected";
                lblStatus.ForeColor = Color.Green;
            }
            else // offline, disconnected, or false
            {
                lblStatus.Text = "● Disconnected";
                lblStatus.ForeColor = Color.Red;
            }
        }

    }
}
