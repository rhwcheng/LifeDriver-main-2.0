﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StyexFleetManagement.Abstractions
{
    public interface IBaseStore<T>
    {
        string Identifier { get; }
        Task InitializeStoreAsync();
        Task<IEnumerable<T>> GetItemsAsync(int skip = 0, int take = 100, bool forceRefresh = false);
        Task<T> GetItemAsync(string id);
        Task<bool> InsertAsync(T item);
        Task<bool> UpdateAsync(T item);
        Task<bool> RemoveAsync(T item);
        Task<bool> RemoveItemsAsync(IEnumerable<T> items);
        Task<bool> SyncAsync();
        Task<bool> DropTable();
    }
}
