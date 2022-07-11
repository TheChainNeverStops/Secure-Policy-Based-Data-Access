    public class PolicyModel
    {
        public int GenericType { get; set; }
        public string Actor { get; set; }
        public string Issuer { get; set; }
        public string Note { get; set; } = "";
        public DateTime ToDate { get; set; } = DateTime.UtcNow.AddDays(15);
        public string GenericKey { get; set; } = "";
        public string UserName { get; set; } = "";
        public string Email { get; set; } = "";
    }
