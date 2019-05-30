using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlGenerator
{
    public class SystemCode
    {
        private List<SystemCodeItem> _CodeItems;
        private string _CodeKey;
        private string _CodeDescription;


        public SystemCode(string codeKey, string codeDescription)
        {
            if (String.IsNullOrWhiteSpace(codeKey))
            {
                throw new ArgumentNullException();
            }

            _CodeKey = codeKey.Trim();
            _CodeDescription = codeDescription.Trim();
            _CodeItems = new List<SystemCodeItem>();
        }


        public string CodeKey
        {
            get { return _CodeKey; }
        }
        public string CodeDescription
        {
            get { return _CodeDescription; }
        }

        public List<SystemCodeItem> CodeItems
        {
            get { return _CodeItems; }
        }

    
        public void Add(SystemCodeItem item)
        {
            if (!_CodeItems.Exists(x => x.Code == item.Code))
            {
                _CodeItems.Add(item);
            }
            else
            {
                throw new InvalidOperationException(String.Format("Key has exists. {0}=>{1}", _CodeKey, item.Code));                            
            }
        }
    }
}
