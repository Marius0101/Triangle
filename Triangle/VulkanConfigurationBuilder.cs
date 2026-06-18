using Silk.NET.Windowing;
using Triangle;

class VulkanConfigurationBuilder
{
    private bool enableValidation;

    public VulkanConfigurationBuilder EnableValidation(bool value)
    {
        enableValidation = value;
        return this;
    }

    public VulkanConfiguration Build(IWindow? window)
    {
        return new VulkanConfiguration(window, enableValidation);
    }
}