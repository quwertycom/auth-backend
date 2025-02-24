namespace API.Common.Utilities;

public interface IMockServices
{
    void MockOperation();
}

public class TestingMockServices : IMockServices
{
    public void MockOperation()
    {
        // Mock implementation for testing
    }
} 