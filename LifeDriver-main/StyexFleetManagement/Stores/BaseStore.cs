﻿using StyexFleetManagement.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StyexFleetManagement.Stores
{
    public class BaseStore<T> : IBaseStore<T>
    {
        #region IBaseStore implementation

        public virtual Task InitializeStoreAsync()
        {
            throw new NotImplementedException();
        }

        public virtual Task<IEnumerable<T>> GetItemsAsync(int skip = 0, int take = 100, bool forceRefresh = false)
        {
            throw new NotImplementedException();
        }

        public virtual Task<T> GetItemAsync(string id)
        {
            throw new NotImplementedException();
        }

        public virtual Task<bool> InsertAsync(T item)
        {
            throw new NotImplementedException();
        }

        public virtual Task<bool> UpdateAsync(T item)
        {
            throw new NotImplementedException();
        }

        public virtual Task<bool> RemoveAsync(T item)
        {
            throw new NotImplementedException();
        }

        public virtual Task<bool> RemoveItemsAsync(IEnumerable<T> items)
        {
            throw new NotImplementedException();
        }

        public virtual Task<bool> SyncAsync()
        {
            return Task.FromResult(true);
        }

        public virtual Task<bool> PullLatestAsync()
        {
            return Task.FromResult(true);
        }

        public virtual Task<bool> DropTable()
        {
            throw new NotImplementedException();
        }

        public string Identifier => "store";

        #endregion
    }
}
