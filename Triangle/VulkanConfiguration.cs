using System.Runtime.InteropServices;
using Silk.NET.Core;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.EXT;
using Silk.NET.Windowing;

namespace Triangle
{
    public unsafe class VulkanConfiguration
    {
        public Vk? vk;
        public Instance instance;
        public ExtDebugUtils? debugUtils;
        public bool enableValidation;
        public DebugUtilsMessengerEXT debugMessenger;
        private readonly string[] validationLayers = new[]
        {
            "VK_LAYER_KHRONOS_validation"
        };
        
        public VulkanConfiguration(IWindow? window, bool enableValidation)
        {
            vk = Vk.GetApi();
            this.enableValidation = enableValidation;

            CreateInstance(window);
            if(enableValidation)
                SetupDebugMessenger();
        }

        private void CreateInstance(IWindow? window)
        {
            if (enableValidation && !CheckValidationLayerSupport())
                throw new Exception("Validation layers requested, but not available!");

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
            var extensions = GetRequiredExtensions(window);
            createInfo.EnabledExtensionCount = (uint)extensions.Length;
            createInfo.PpEnabledExtensionNames = (byte**)SilkMarshal.StringArrayToPtr(extensions); ;
            
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
            if (vk!.CreateInstance(in createInfo, null, out instance) != Result.Success)
                throw new Exception("failed to create instance!");

            Marshal.FreeHGlobal((IntPtr)appInfo.PApplicationName);
            Marshal.FreeHGlobal((IntPtr)appInfo.PEngineName);
            SilkMarshal.Free((nint)createInfo.PpEnabledExtensionNames);
            if (enableValidation)
                SilkMarshal.Free((nint)createInfo.PpEnabledLayerNames);
        }


        private void SetupDebugMessenger()
        {
           if (!vk!.TryGetInstanceExtension(instance, out debugUtils))
                return;
           DebugUtilsMessengerCreateInfoEXT debugCreateInfo = new();    
           PopulateDebugMessengerCreateInfo(ref debugCreateInfo); 
           if (debugUtils!.CreateDebugUtilsMessenger(instance, in debugCreateInfo, null, out debugMessenger) != Result.Success)
            {
                throw new Exception("failed to set up debug messenger!");
            }
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
        private string[] GetRequiredExtensions(IWindow window)
        {
            var glfwExtensions = window!.VkSurface!.GetRequiredExtensions(out var glfwExtensionCount);
            var extensions = SilkMarshal.PtrToStringArray((nint)glfwExtensions, (int)glfwExtensionCount);

            if(enableValidation)
                return extensions.Append(ExtDebugUtils.ExtensionName).ToArray();
            return extensions;
        }
        private void PopulateDebugMessengerCreateInfo(ref DebugUtilsMessengerCreateInfoEXT debugCreateInfo)
        {
            debugCreateInfo.SType = StructureType.DebugUtilsMessengerCreateInfoExt;
            debugCreateInfo.MessageSeverity = DebugUtilsMessageSeverityFlagsEXT.VerboseBitExt |
                                     DebugUtilsMessageSeverityFlagsEXT.WarningBitExt |
                                     DebugUtilsMessageSeverityFlagsEXT.ErrorBitExt;
            debugCreateInfo.MessageType = DebugUtilsMessageTypeFlagsEXT.GeneralBitExt |
                                 DebugUtilsMessageTypeFlagsEXT.PerformanceBitExt |
                                 DebugUtilsMessageTypeFlagsEXT.ValidationBitExt;
            debugCreateInfo.PfnUserCallback = new PfnDebugUtilsMessengerCallbackEXT(DebugCallback);
        }

        private uint DebugCallback(DebugUtilsMessageSeverityFlagsEXT messageSeverity, DebugUtilsMessageTypeFlagsEXT messageTypes, DebugUtilsMessengerCallbackDataEXT* pCallbackData, void* pUserData)
        {
            string message = Marshal.PtrToStringAnsi((nint)pCallbackData->PMessage);
            string idName = Marshal.PtrToStringAnsi((nint)pCallbackData->PMessageIdName);
            Console.WriteLine($"[{messageSeverity}] [{messageTypes}] {idName}: {message}");

            return Vk.False;
        }
    }
}