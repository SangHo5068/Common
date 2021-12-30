using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Common.Notify;

namespace Common.Demo
{
    public enum ItemUIType
    {
        Header,
        Item
    }

    public interface IDataInfo
    {
        ItemUIType UIType { get; set; }
        string Name { get; set; }
    }

    public class DataInfo : BindableAndDisposable, IDataInfo
    {
        private ItemUIType _UIType = ItemUIType.Item;
        private string _Name;

        public ItemUIType UIType {
            get => _UIType;
            set => SetValue(ref _UIType, value);
        }
        public string Name
        {
            get => _Name;
            set => SetValue(ref _Name, value);
        }
        public object Data { get; private set; }



        internal void SetSignalData(object data)
        {
            Data = data;
        }
    }

    public class DataHeaderInfo : BindableAndDisposable, IDataInfo
    {
        private ItemUIType _UIType = ItemUIType.Header;
        private string _Name;

        public ItemUIType UIType
        {
            get => _UIType;
            set => SetValue(ref _UIType, value);
        }
        public string Name
        {
            get => _Name;
            set => SetValue(ref _Name, value);
        }
    }


    public class SubscribeDataInfo
    {
        public string Name { get; set; }
        public object Data { get; set; }
    }
}
