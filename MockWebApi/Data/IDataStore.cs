using MockWebApi.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MockWebApi.Data
{
    public interface IDataStore
    {

        void Store(RequestInformation information);

        void Clear();

        RequestInformation GetInformation(string id);

        RequestInformation[] GetAllInformation(int? count);

    }
}
