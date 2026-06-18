using System.Runtime.InteropServices;
using Silk.NET.Core;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using Silk.NET.Windowing;

namespace Triangle
{
    public unsafe class VulkanConfiguration
    {
        public Vk? vk;
        public Instance instance;
        private readonly string[] validationLayers = new[]
        {
            "VK_LAYER_KHRONOS_validation"
        };
        public VulkanConfiguration(IWindow? window, bool enableValidation)
        {
            vk = Vk.GetApi();

            CreateInstance(window, enableValidation);
            if(enableValidation)
                SetupDebugMessenger();
        }

        private void CreateInstance(IWindow? window, bool enableValidation)
        {
            if (enableValidation && !CheckValidationLayerSupport())
                throw new Exception("validation layers requested, but not available!");

            ApplicationInfo appInfo = new()
            {
                SType = StructureType.ApplicationInfo,
                PApplicationName = (byte*)Marshal.StringToHGlobalAnsi("Triangle"),
                ApplicationVersion = new Version32(1, 0, 0),
                PEngineName = (byte*)Marshal.StringToHGlobalAnsi("No Engine"),
                EngineVersion = new Version32(1, 0, 0),
                ApiVersion = Vk.Version12
            };
            InstanceCreateInfo createInfo = new()
            {
                SType = StructureType.InstanceCreateInfo,
                PApplicationInfo = &appInfo
            };
            var glfwExtensions = window!.VkSurface!.GetRequiredExtensions(out var glfwExtensionCount);
            createInfo.EnabledExtensionCount = glfwExtensionCount;
            createInfo.PpEnabledExtensionNames = glfwExtensions;
            if (enableValidation)
            {
                createInfo.EnabledLayerCount = (uint)validationLayers.Length;
                createInfo.PpEnabledLayerNames = (byte**)SilkMarshal.StringArrayToPtr(validationLayers);

                DebugUtilsMessengerCreateInfoEXT debugCreateInfo = new();
                PopulateDebugMessengerCreateInfo(ref debugCreateInfo);
                createInfo.PNext = &debugCreateInfo;
            }
            else
            {
                createInfo.EnabledLayerCount = 0;
                createInfo.PNext = null;
            }

            createInfo.EnabledLayerCount = 0;
            if (vk!.CreateInstance(in createInfo, null, out instance) != Result.Success)
                throw new Exception("failed to create instance!");

            Marshal.FreeHGlobal((IntPtr)appInfo.PApplicationName);
            Marshal.FreeHGlobal((IntPtr)appInfo.PEngineName);
            if (enableValidation)
                SilkMarshal.Free((nint)createInfo.PpEnabledLayerNames);
        }

        private void SetupDebugMessenger()
        {
            throw new NotImplementedException();
        }

        private bool CheckValidationLayerSupport()
        {
            uint layerCount = 0;
            vk!.EnumerateInstanceLayerProperties(ref layerCount, null);
            var availableLayers = new LayerProperties[layerCount];
            fixed (LayerProperties* availableLayersPtr = availableLayers)
            {
                vk!.EnumerateInstanceLayerProperties(ref layerCount, availableLayersPtr);
            }
            var availableLayerNames = availableLayers.Select(layer => Marshal.PtrToStringAnsi((IntPtr)layer.LayerName)).ToHashSet();

            return validationLayers.All(availableLayerNames.Contains);
        }
    }
}