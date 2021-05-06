using System;
using System.Collections.Generic;
using System.Linq;

using Common.Base;
using Common.Views;

namespace Common.Models
{
    public class ModuleDescription
    {
        public Type ModuleType { get; set; }
        public Type ViewType { get; set; }
        public Type NavigationParentModuleType { get; set; }
    }
    public class ModuleDescriptionCollection : List<ModuleDescription>, IViewsListProvider, IModulesListProvider
    {
        Type mainModuleType = null;
        public Type GetMainModuleType()
        {
            if (mainModuleType == null)
                mainModuleType = GetMainModuleTypeCore();
            return mainModuleType;
        }
        Type GetMainModuleTypeCore()
        {
            return (from t in this where t.ModuleType != null && t.ModuleType.IsSubclassOf(typeof(BaseMainControlViewModel)) select t.ModuleType).SingleOrDefault();
        }
        Type IModulesListProvider.GetNavigationParentModuleType(Type moduleType)
        {
            if (moduleType == null || moduleType == GetMainModuleType()) return null;
            Type navigationParentModuleType = (from t in this where t.ModuleType == moduleType select t.NavigationParentModuleType).SingleOrDefault();
            return navigationParentModuleType ?? GetMainModuleType();
        }
        Type IViewsListProvider.GetViewType(Type moduleType)
        {
            if (moduleType == null) return null;
            return (from t in this where t.ModuleType == moduleType select t.ViewType).SingleOrDefault();
        }
    }
}
