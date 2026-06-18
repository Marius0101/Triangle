using System.Runtime.InteropServices;
using Silk.NET.Core;
using Silk.NET.Maths;
using Silk.NET.Vulkan;
using Silk.NET.Windowing;
using Triangle;

namespace Triangle
{
    public unsafe class TriangleApplication
    {
        private const int width = 800;
        private const int height = 600;
        private IWindow? window;
        private VulkanConfiguration vulkanConfiguration;
        public void Run()
        {
            InitWindow();
            InitVulkan();
            MainLoop();
            CleanUp();
        }

        private void InitWindow()
        {
            var options = WindowOptions.DefaultVulkan with
            {
                Size = new Vector2D<int>(width, height),
                Title = "Triangle"
            };
            window = Window.Create(options);
            window.Initialize();

            if (window.VkSurface is null)
                throw new Exception("Windowing platform doesn't support Vulkan.");
        }
        private void InitVulkan()
        {
            vulkanConfiguration = new VulkanConfigurationBuilder()
                .EnableValidation(true)
                .Build(window);
        }

        private void MainLoop()
        {
            window!.Run();
        }

        private void CleanUp()
        {
            vulkanConfiguration.vk?.DestroyInstance(vulkanConfiguration.instance, null);
            vulkanConfiguration.vk?.Dispose();

            window?.Dispose();
        }  
    }
}