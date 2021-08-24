using MockWebApi.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MockWebApi.Data
{
    public interface IRequestHistory
    {

        void Store(RequestHistoryItem information);

        void Clear();

        RequestHistoryItem GetInformation(string id);

        RequestHistoryItem[] GetAllInformation(int? count);

    }
}
