using System;
using System.Collections.Generic;

namespace EnjoyJob.Battle
{
    // pets.json 的根对象，对应最外层 { "pets": [...] }。
    [Serializable]
    public class PetTable
    {
        public List<PetRecord> pets = new List<PetRecord>();
    }
}
