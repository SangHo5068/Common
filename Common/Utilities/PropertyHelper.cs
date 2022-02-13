using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

using Data = System.Data;


namespace Common.Utilities
{
    public static class PropertyHelper
    {
        public class PropertyOverridingTypeDescriptor : CustomTypeDescriptor
        {
            private readonly Dictionary<string, PropertyDescriptor> overridePds = new Dictionary<string, PropertyDescriptor>();

            public PropertyOverridingTypeDescriptor(ICustomTypeDescriptor parent)
                : base(parent)
            { }

            public void OverrideProperty(PropertyDescriptor pd)
            {
                overridePds[pd.Name] = pd;
            }

            public override object GetPropertyOwner(PropertyDescriptor pd)
            {
                object o = base.GetPropertyOwner(pd);

                if (o == null)
                {
                    return this;
                }

                return o;
            }

            public PropertyDescriptorCollection GetPropertiesImpl(PropertyDescriptorCollection pdc)
            {
                List<PropertyDescriptor> pdl = new List<PropertyDescriptor>(pdc.Count + 1);

                foreach (PropertyDescriptor pd in pdc)
                {
                    if (overridePds.ContainsKey(pd.Name))
                    {
                        pdl.Add(overridePds[pd.Name]);
                    }
                    else
                    {
                        pdl.Add(pd);
                    }
                }

                PropertyDescriptorCollection ret = new PropertyDescriptorCollection(pdl.ToArray());

                return ret;
            }

            public override PropertyDescriptorCollection GetProperties()
            {
                return GetPropertiesImpl(base.GetProperties());
            }
            public override PropertyDescriptorCollection GetProperties(Attribute[] attributes)
            {
                return GetPropertiesImpl(base.GetProperties(attributes));
            }
        }

        public class TypeDescriptorOverridingProvider : TypeDescriptionProvider
        {
            private readonly ICustomTypeDescriptor ctd;

            public TypeDescriptorOverridingProvider(ICustomTypeDescriptor ctd)
            {
                this.ctd = ctd;
            }

            public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance)
            {
                return ctd;
            }
        }

        public static void SetAttribute(object selectedObject, string pname, string category)
        {
            // prepare our property overriding type descriptor
            PropertyOverridingTypeDescriptor ctd =
                new PropertyOverridingTypeDescriptor(TypeDescriptor.GetProvider(selectedObject).GetTypeDescriptor(selectedObject));

            // iterate through properies in the supplied object/type
            foreach (PropertyDescriptor pd in TypeDescriptor.GetProperties(selectedObject))
            {
                // for every property that complies to our criteria
                if (pd.Name.EndsWith(pname))
                {
                    // we first construct the custom PropertyDescriptor with the TypeDescriptor's
                    // built-in capabilities
                    PropertyDescriptor pd2 =
                        TypeDescriptor.CreateProperty(
                            selectedObject.GetType(), // or just _settings, if it's already a type
                            pd, // base property descriptor to which we want to add attributes
                                // The PropertyDescriptor which we'll get will just wrap that
                                // base one returning attributes we need.
                                new CategoryAttribute(category)
                        //new EditorAttribute( // the attribute in question
                        //    typeof(System.Web.UI.Design.ConnectionStringEditor),
                        //    typeof(System.Drawing.Design.UITypeEditor)
                        //)
                        // this method really can take as many attributes as you like,
                        // not just one
                        );

                    // and then we tell our new PropertyOverridingTypeDescriptor to override that property
                    ctd.OverrideProperty(pd2);
                }
            }

            // then we add new descriptor provider that will return our descriptor istead of default
            TypeDescriptor.AddProvider(new TypeDescriptorOverridingProvider(ctd), selectedObject);
        }

        public static bool TryGetAttribute<T>(MemberInfo memberInfo, out T customAttribute) where T : Attribute
        {
            var attributes = memberInfo.GetCustomAttributes(typeof(T), false).FirstOrDefault();
            if (attributes == null)
            {
                customAttribute = null;
                return false;
            }
            customAttribute = (T)attributes;
            return true;
        }

        public static void FillDataTable<T>(Data.DataTable dt, List<T> items)
        {
            var propList = typeof(T).GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            foreach (T t in items)
            {
                var row = dt.NewRow();
                foreach (MemberInfo info in propList)
                {
                    if (info is PropertyInfo)
                        row[info.Name] = (info as PropertyInfo).GetValue(t, null);
                    else if (info is FieldInfo)
                        row[info.Name] = (info as FieldInfo).GetValue(t);
                }
                dt.Rows.Add(row);
            }
        }

        public static Data.DataTable CreateDataTable<T>()
        {
            var dt = new Data.DataTable();

            var propList = typeof(T).GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            foreach (MemberInfo info in propList)
            {
                if (info is PropertyInfo)
                    dt.Columns.Add(new Data.DataColumn(info.Name, (info as PropertyInfo).PropertyType));
                else if (info is FieldInfo)
                    dt.Columns.Add(new Data.DataColumn(info.Name, (info as FieldInfo).FieldType));
            }

            return dt;
        }

        // function that creates a list of an object from the given data table
        public static List<T> CreateListFromTable<T>(Data.DataTable tbl) where T : new()
        {
            // define return list
            List<T> lst = new List<T>();

            // go through each row
            foreach (Data.DataRow r in tbl.Rows)
            {
                // add to the list
                lst.Add(CreateItemFromRow<T>(r));
            }

            // return the list
            return lst;
        }

        // function that creates an object from the given data row
        public static T CreateItemFromRow<T>(Data.DataRow row) where T : new()
        {
            // create a new object
            T item = new T();

            // set the item
            SetItemFromRow(item, row);

            // return 
            return item;
        }

        public static void SetItemFromRow<T>(T item, Data.DataRow row) where T : new()
        {
            // go through each column
            foreach (Data.DataColumn c in row.Table.Columns)
            {
                // find the property for the column
                //PropertyInfo p = item.GetType().GetProperty(c.ColumnName);
                PropertyInfo p = item.GetType().GetProperties().FirstOrDefault(f => f.Name.ToLower() == c.ColumnName.ToLower());

                // if exists, set the value
                if (p != null && row[c] != DBNull.Value)
                {
                    p.SetValue(item, row[c], null);
                }
            }
        }

        #region Sample
        //public static class Program
        //{
        //    public static void Main()
        //    {
        //        Object selectedObject = new myDataClass
        //        {
        //            x = 100,
        //            y = 200
        //        };

        //        CategoryAttribute attrx =
        //           selectedObject.GetType().GetProperty("x").GetCustomAttributes(typeof(CategoryAttribute), false).Single() as
        //           CategoryAttribute;

        //        FieldInfo category_x = attrx.GetType().GetField("categoryValue", BindingFlags.NonPublic | BindingFlags.Instance);
        //        if (category_x != null)
        //        {
        //            if (category_x.FieldType == "string".GetType())
        //            {
        //                category_x.SetValue(attrx, "categoryX");
        //            }
        //        }

        //        Debug.Assert(attrx.Category == "categoryX");

        //        CategoryAttribute attry =
        //           selectedObject.GetType().GetProperty("y").GetCustomAttributes(typeof(CategoryAttribute), false).Single() as
        //           CategoryAttribute;

        //        FieldInfo category_y = attry.GetType().GetField("categoryValue", BindingFlags.NonPublic | BindingFlags.Instance);
        //        if (category_y != null)
        //        {
        //            if (category_y.FieldType == "string".GetType())
        //            {
        //                category_y.SetValue(attry, "categoryY");
        //            }
        //        }

        //        Debug.Assert(attrx.Category == "categoryX"); //success now!
        //        Debug.Assert(attry.Category == "categoryY");
        //    }

        //    public partial class myDataClass
        //    {
        //        [CategoryAttribute("Test")]
        //        public int x { get; set; }

        //        [CategoryAttribute("Default")]
        //        public int y { get; set; }
        //    }
        //}
        #endregion //Sample
    }



    /// <summary>
    /// ex)
    /// var pretty = ViewModel<Report>.DressUp(datum);
    /// pretty.PropertyAttributeReplacements[typeof(Smiley)] = new List<Attribute>() { new EditorAttribute(typeof(SmileyEditor),typeof(UITypeEditor))};
    /// propertyGrid1.SelectedObject = pretty;
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ViewModel<T> : CustomTypeDescriptor
    {
        private T _instance;
        private ICustomTypeDescriptor _originalDescriptor;
        public ViewModel(T instance, ICustomTypeDescriptor originalDescriptor) : base(originalDescriptor)
        {
            _instance = instance;
            _originalDescriptor = originalDescriptor;
            PropertyAttributeReplacements = new Dictionary<Type, IList<Attribute>>();
        }

        public static ViewModel<T> DressUp(T instance)
        {
            return new ViewModel<T>(instance, TypeDescriptor.GetProvider(instance).GetTypeDescriptor(instance));
        }

        /// <summary>
        /// Most useful for changing EditorAttribute and TypeConvertorAttribute
        /// </summary>
        public IDictionary<Type, IList<Attribute>> PropertyAttributeReplacements { get; set; }

        public override PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            var properties = base.GetProperties(attributes).Cast<PropertyDescriptor>();

            var bettered = properties.Select(pd =>
            {
                if (PropertyAttributeReplacements.ContainsKey(pd.PropertyType))
                {
                    return TypeDescriptor.CreateProperty(typeof(T), pd, PropertyAttributeReplacements[pd.PropertyType].ToArray());
                }
                else
                {
                    return pd;
                }
            });
            return new PropertyDescriptorCollection(bettered.ToArray());
        }

        public override PropertyDescriptorCollection GetProperties()
        {
            return GetProperties(null);
        }
    }

    internal class PropertyOverridingTypeDescriptionProvider : TypeDescriptionProvider
    {
        private readonly Dictionary<Type, ICustomTypeDescriptor> _descriptorCache = new Dictionary<Type, ICustomTypeDescriptor>();
        private readonly Func<PropertyDescriptor, bool> _condition;
        private readonly Func<PropertyDescriptor, Type, PropertyDescriptor> _propertyCreator;

        public PropertyOverridingTypeDescriptionProvider(TypeDescriptionProvider parentProvider, Func<PropertyDescriptor, bool> condition, Func<PropertyDescriptor, Type, PropertyDescriptor> propertyCreator) : base(parentProvider)
        {
            _condition = condition;
            _propertyCreator = propertyCreator;
        }

        public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance)
        {
            lock (_descriptorCache)
            {
                if (!_descriptorCache.TryGetValue(objectType, out ICustomTypeDescriptor returnDescriptor))
                {
                    returnDescriptor = CreateTypeDescriptor(objectType);
                }
                return returnDescriptor;
            }
        }

        private ICustomTypeDescriptor CreateTypeDescriptor(Type targetType)
        {
            var descriptor = base.GetTypeDescriptor(targetType, null);
            _descriptorCache.Add(targetType, descriptor);
            var ctd = new PropertyOverridingTypeDescriptor(descriptor, targetType, _condition, _propertyCreator);
            _descriptorCache[targetType] = ctd;
            return ctd;
        }
    }

    internal class PropertyOverridingTypeDescriptor : CustomTypeDescriptor
    {
        private readonly ICustomTypeDescriptor _parent;
        private readonly PropertyDescriptorCollection _propertyCollection;
        private readonly Type _objectType;
        private readonly Func<PropertyDescriptor, bool> _condition;
        private readonly Func<PropertyDescriptor, Type, PropertyDescriptor> _propertyCreator;

        public PropertyOverridingTypeDescriptor(ICustomTypeDescriptor parent, Type objectType, Func<PropertyDescriptor, bool> condition, Func<PropertyDescriptor, Type, PropertyDescriptor> propertyCreator)
            : base(parent)
        {
            _parent = parent;
            _objectType = objectType;
            _condition = condition;
            _propertyCreator = propertyCreator;
            _propertyCollection = BuildPropertyCollection();
        }

        private PropertyDescriptorCollection BuildPropertyCollection()
        {
            var isChanged = false;
            var parentProperties = _parent.GetProperties();

            var pdl = new PropertyDescriptor[parentProperties.Count];
            var index = 0;
            foreach (var pd in parentProperties.OfType<PropertyDescriptor>())
            {
                var pdReplaced = pd;
                if (_condition(pd))
                {
                    pdReplaced = _propertyCreator(pd, _objectType);
                }
                if (!ReferenceEquals(pdReplaced, pd)) isChanged = true;
                pdl[index++] = pdReplaced;
            }
            return !isChanged ? parentProperties : new PropertyDescriptorCollection(pdl);
        }

        public override object GetPropertyOwner(PropertyDescriptor pd)
        {
            var o = base.GetPropertyOwner(pd);
            return o ?? this;
        }

        public override PropertyDescriptorCollection GetProperties()
        {
            return _propertyCollection;
        }
        public override PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            return _propertyCollection;
        }
    }



    public class ToolbarImageEditorExtender : Component
    {
        private static bool _alreadyInitialized;
        public ToolbarImageEditorExtender()
        {
            // no need to reinitialize if we drop more than one component
            if (_alreadyInitialized)
                return;
            _alreadyInitialized = true;
            OtherControl other = new OtherControl();
            AButton abtn = new AButton();
            // the ChangeTypeProperties function above. I just made a generic version
            ChangeTypeProperties(other, nameof(OtherControl.Glyph), nameof(OtherControl.LargeGlyph));
            ChangeTypeProperties(abtn, nameof(AButton.SmallImage), nameof(AButton.LargeImage));
            // etc.
        }

        private void ChangeTypeProperties<T>(T modifiedType, params string[] propertyNames)
        {
            // Get the current TypeDescriptionProvider
            var curProvider = TypeDescriptor.GetProvider(modifiedType);
            // Create a replacement provider, pass in the parent, this is important
            var replaceProvider = new PropertyOverridingTypeDescriptionProvider(curProvider,
                // This the predicate that says wether a `PropertyDescriptor` should be changed
                // Here we are changing only the System.Drawing.Image properties,
                // either those whose name we pass in, or all if we pass none
                pd =>
                    typeof(System.Drawing.Image).IsAssignableFrom(pd.PropertyType) &&
                    (propertyNames.Length == 0 || propertyNames.Contains(pd.Name)),

                // This our "replacer" function. It'll get the source PropertyDescriptor and the object type.
                // You could use pd.ComponentType for the object type, but I've
                // found it to fail under some circumstances, so I just pass it
                // along
                (pd, t) =>
                {
                    // Get original attributes except the ones we want to change
                    var atts = pd.Attributes.OfType<Attribute>().Where(x => x.GetType() != typeof(EditorAttribute)).ToList();
                    // Add our own attributes
                    atts.Add(new EditorAttribute(typeof(T), typeof(System.Drawing.Design.UITypeEditor)));
                    // Create the new PropertyDescriptor
                    return TypeDescriptor.CreateProperty(t, pd, atts.ToArray());
                }
            );
            // Finally we replace the TypeDescriptionProvider
            TypeDescriptor.AddProvider(replaceProvider, modifiedType);
        }

        private class OtherControl
        {
            public static object Glyph { get; internal set; }
            public static object LargeGlyph { get; internal set; }
        }

        private class AButton
        {
            public static object SmallImage { get; internal set; }
            public static object LargeImage { get; internal set; }
        }
    }



    #region DynamicTypeCreator
    public interface IBaseObject
    {
        IEmptyObject AddPassThroughCtors();
    }
    public interface IEmptyObject
    {
        IAfterProperty AddProperty(string name, Type type);
        IAfterProperty AddPropertys(Dictionary<string, Type> pairs);
    }
    public interface IAfterProperty : IEmptyObject, IFinishBuild
    {
        IAfterAttribute AddPropertyAttribute(Type attrType, Type[] ctorArgTypes, params object[] ctorArgs);
    }
    public interface IAfterAttribute : IEmptyObject, IFinishBuild { }
    public interface IFinishBuild
    {
        Type FinishBuildingType();
        Type FinishBuildingAndSaveType(string assemblyFileName);
    }
    public static class DynamicTypeCreator
    {
        public static IBaseObject Create(string className, Type parentType)
        {
            return new DynamicTypeCreatorBase().Create(className, parentType);
        }
        public static IBaseObject Create(string className, Type parentType, string dir)
        {
            return new DynamicTypeCreatorBase().Create(className, parentType, dir);
        }
    }
    public class PropertyBuilding
    {
        public PropertyBuilding(PropertyBuilder propertyBuild, MethodBuilder getBuild, MethodBuilder setBuild)
        {
            propertyBuilder = propertyBuild;
            getBuilder = getBuild;
            setBuilder = setBuild;
        }
        public PropertyBuilder propertyBuilder { get; }
        public MethodBuilder getBuilder { get; }
        public MethodBuilder setBuilder { get; }
    }
    public class DynamicTypeCreatorBase : IBaseObject, IEmptyObject, IAfterProperty, IAfterAttribute
    {
        TypeBuilder _tBuilder;
        List<PropertyBuilding> _propBuilds = new List<PropertyBuilding>();
        AssemblyBuilder _aBuilder;
        private Type _parentType;

        /// <summary>
        /// Begins creating type using the specified name.
        /// </summary>
        /// <param name="className">Class name for new type</param>
        /// <param name="parentType">Name of base class. Use object if none</param>
        /// <returns></returns>
        public IBaseObject Create(string className, Type parentType)
        {
            return Create(className, parentType, "");
        }
        /// <summary>
        /// Begins creating type using the specified name and saved in the specified directory.
        /// Use this overload to save the resulting .dll in a specified directory.
        /// </summary>
        /// <param name="className">Class name for new type</param>
        /// <param name="parentType">Name of base class. Use object if none</param>
        /// <param name="dir">Directory path to save .dll in</param>
        /// <returns></returns>
        public IBaseObject Create(string className, Type parentType, string dir)
        {
            _parentType = parentType;
            //Define type
            AssemblyName _name = new AssemblyName(className);
            if (string.IsNullOrWhiteSpace(dir))
            {
                _aBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(_name, AssemblyBuilderAccess.RunAndSave);
            }
            else _aBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(_name, AssemblyBuilderAccess.RunAndSave, dir);
            ModuleBuilder _mBuilder = _aBuilder.DefineDynamicModule(_name.Name, _name.Name + ".dll");
            _tBuilder = _mBuilder.DefineType(className, TypeAttributes.Public | TypeAttributes.Class, parentType);
            return this;
        }
        /// <summary>
        /// Adds constructors to new type that match all constructors on base type.
        /// Parameters are passed to base type.
        /// </summary>
        /// <returns></returns>
        public IEmptyObject AddPassThroughCtors()
        {
            foreach (ConstructorInfo _ctor in _parentType.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                ParameterInfo[] _params = _ctor.GetParameters();
                Type[] _paramTypes = _params.Select(p => p.ParameterType).ToArray();
                Type[][] _reqModifiers = _params.Select(p => p.GetRequiredCustomModifiers()).ToArray();
                Type[][] _optModifiers = _params.Select(p => p.GetOptionalCustomModifiers()).ToArray();
                ConstructorBuilder _ctorBuild = _tBuilder.DefineConstructor(MethodAttributes.Public, _ctor.CallingConvention, _paramTypes, _reqModifiers, _optModifiers);
                for (int i = 0; i < _params.Length; i++)
                {
                    ParameterInfo _param = _params[i];
                    ParameterBuilder _prmBuild = _ctorBuild.DefineParameter(i + 1, _param.Attributes, _param.Name);
                    if (((int)_param.Attributes & (int)ParameterAttributes.HasDefault) != 0) _prmBuild.SetConstant(_param.RawDefaultValue);

                    foreach (CustomAttributeBuilder _attr in GetCustomAttrBuilders(_param.CustomAttributes))
                    {
                        _prmBuild.SetCustomAttribute(_attr);
                    }
                }

                //ConstructorBuilder _cBuilder = _tBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Any, argTypes);
                ILGenerator _ctorGen = _ctorBuild.GetILGenerator();
                _ctorGen.Emit(OpCodes.Nop);
                //arg0=new obj, arg1-arg3=passed params. Push onto stack for call to base class
                _ctorGen.Emit(OpCodes.Ldarg_0);
                for (int i = 1; i <= _params.Length; i++) _ctorGen.Emit(OpCodes.Ldarg, i);
                _ctorGen.Emit(OpCodes.Call, _ctor);
                _ctorGen.Emit(OpCodes.Ret);
            }
            return this;
        }
        /// <summary>
        /// Adds a new property to type with specified name and type.
        /// </summary>
        /// <param name="name">Name of property</param>
        /// <param name="type">Type of property</param>
        /// <returns></returns>
        public IAfterProperty AddProperty(string name, Type type)
        {
            var building = NewBuildingProp(name, type);
            _propBuilds.Add(building);
            return this;
        }

        public IAfterProperty AddPropertys(Dictionary<string, Type> pairs)
        {
            foreach (var item in pairs)
            {
                var building = NewBuildingProp(item.Key, item.Value);
                _propBuilds.Add(building);
            }
            return this;
        }

        private PropertyBuilding NewBuildingProp(string name, Type type)
        {
            PropertyBuilder _pBuilder;
            MethodBuilder _getBuilder, _setBuilder;
            //base property
            _pBuilder = _tBuilder.DefineProperty(name, PropertyAttributes.None, type, new Type[0] { });
            //backing field
            FieldBuilder _fBuilder = _tBuilder.DefineField($"m_{name}", type, FieldAttributes.Private);

            //get method
            MethodAttributes _propAttrs = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig;
            _getBuilder = _tBuilder.DefineMethod($"get_{name}", _propAttrs, type, Type.EmptyTypes);
            ILGenerator _getGen = _getBuilder.GetILGenerator();
            _getGen.Emit(OpCodes.Ldarg_0);
            _getGen.Emit(OpCodes.Ldfld, _fBuilder);
            _getGen.Emit(OpCodes.Ret);

            //set method
            _setBuilder = _tBuilder.DefineMethod($"set_{name}", _propAttrs, null, new Type[] { type });
            ILGenerator _setGen = _setBuilder.GetILGenerator();
            _setGen.Emit(OpCodes.Ldarg_0);
            _setGen.Emit(OpCodes.Ldarg_1);
            _setGen.Emit(OpCodes.Stfld, _fBuilder);
            _setGen.Emit(OpCodes.Ret);

            return new PropertyBuilding(_pBuilder, _getBuilder, _setBuilder);
        }

        /// <summary>
        /// Adds an attribute to a property just added.
        /// </summary>
        /// <param name="attrType">Type of attribute</param>
        /// <param name="ctorArgTypes">Types of attribute's cstor parameters</param>
        /// <param name="ctorArgs">Values to pass in to attribute's cstor. Must match in type and order of cstorArgTypes parameter</param>
        /// <returns></returns>
        public IAfterAttribute AddPropertyAttribute(Type attrType, Type[] ctorArgTypes, params object[] ctorArgs)
        {
            if (ctorArgTypes.Length != ctorArgs.Length) throw new Exception("Type count must match arg count for attribute specification");
            ConstructorInfo _attrCtor = attrType.GetConstructor(ctorArgTypes);
            for (int i = 0; i < ctorArgTypes.Length; i++)
            {
                CustomAttributeBuilder _attrBuild = new CustomAttributeBuilder(_attrCtor, ctorArgs);
                _propBuilds.Last().propertyBuilder.SetCustomAttribute(_attrBuild);
            }
            return this;
        }
        /// <summary>
        /// Completes building type, compiles it, and returns the resulting type
        /// </summary>
        /// <returns></returns>
        public Type FinishBuildingType()
        {
            foreach (var _pBuilder in _propBuilds)
            {
                _pBuilder.propertyBuilder.SetGetMethod(_pBuilder.getBuilder);
                _pBuilder.propertyBuilder.SetSetMethod(_pBuilder.setBuilder);
            }

            Type _paramsType = _tBuilder.CreateType();
            return _paramsType;
        }
        /// <summary>
        /// Completes building type, compiles it, saves it, and returns the resultying type.
        /// Assembly is saved in the calling assembly's directory or in the dir specified in the Create method.
        /// </summary>
        /// <param name="assemblyFileName">Filename of the assembly</param>
        /// <returns></returns>
        public Type FinishBuildingAndSaveType(string assemblyFileName)
        {
            Type _newType = FinishBuildingType();
            Save(assemblyFileName);
            return _newType;
        }
        #region Helpers
        private CustomAttributeBuilder[] GetCustomAttrBuilders(IEnumerable<CustomAttributeData> customAttributes)
        {
            return customAttributes.Select(attribute => {
                object[] attributeArgs = attribute.ConstructorArguments.Select(a => a.Value).ToArray();
                PropertyInfo[] namedPropertyInfos = attribute.NamedArguments.Select(a => a.MemberInfo).OfType<PropertyInfo>().ToArray();
                object[] namedPropertyValues = attribute.NamedArguments.Where(a => a.MemberInfo is PropertyInfo).Select(a => a.TypedValue.Value).ToArray();
                FieldInfo[] namedFieldInfos = attribute.NamedArguments.Select(a => a.MemberInfo).OfType<FieldInfo>().ToArray();
                object[] namedFieldValues = attribute.NamedArguments.Where(a => a.MemberInfo is FieldInfo).Select(a => a.TypedValue.Value).ToArray();
                return new CustomAttributeBuilder(attribute.Constructor, attributeArgs, namedPropertyInfos, namedPropertyValues, namedFieldInfos, namedFieldValues);
            }).ToArray();
        }
        /// <summary>
        /// Requires admin privileges. 
        /// To save in a specified dir, use the Create overload that requires a 'dir' parameter.
        /// </summary>
        /// <param name="assemblyFileName"></param>
        private void Save(string assemblyFileName)
        {
            string _assemblyName = assemblyFileName;
            if (!Path.HasExtension(assemblyFileName) || Path.GetExtension(assemblyFileName).ToLower() != ".dll")
                _assemblyName += ".dll";
            _aBuilder.Save(_assemblyName);
        }
        #endregion
    }
    #endregion //DynamicTypeCreator
}
