using System;
using MvvmHelpers;
using ProtoBuf;
using StyexFleetManagement.Helpers;

namespace StyexFleetManagement.Models
{
    public interface IBaseDataObject
    {
        string Id { get; set; }
    }
    
    public class BaseDataObject : ObservableObject, IBaseDataObject
    {
        public BaseDataObject()
        {
            Id = GuidGenerator.GenerateTimeBasedGuid(DateTime.Now).ToString();
        }

        [ProtoMember(1)]
        public string Id { get; set; }
    }
}
