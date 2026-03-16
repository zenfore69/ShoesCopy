using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShoesNet.Model
{
    public partial class Товар
    {
        public string ПолныйПутьКФото
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Фото))
                    return "/Assets/picture.png"; 

                return $"/Assets/{Фото}";
            }
        }
    }
}