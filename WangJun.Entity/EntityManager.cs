using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WangJun.Entity
{
    public class EntityManager
    {
        public static EntityManager GetInstance()
        {
            var inst = new EntityManager();
            return inst;
        }

        
    }
}
