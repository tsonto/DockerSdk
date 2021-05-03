namespace DockerSdk.Networks
{
    public record NetworkSandbox
    {
        public string SandboxId { get; }
        public string SandboxKey { get; }

        internal NetworkSandbox(string sandboxID, string sandboxKey)
        {
            SandboxId = sandboxID;
            SandboxKey = sandboxKey;
        }
    }
}
