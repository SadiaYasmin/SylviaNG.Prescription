namespace SylviaNG.Prescription.Infrastructure.Kafka
{
    public class KafkaSettings
    {
        public string BootstrapServers { get; set; } = string.Empty;
        public string GroupId { get; set; } = "sylviang-prescription-employee-sync";
    }
}
