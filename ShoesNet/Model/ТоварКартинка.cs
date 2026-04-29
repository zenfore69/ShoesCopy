using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShoesNet.Model
{
    public partial class Товар
    {

        public Nullable<decimal> ЦенаСоСкидкой
        {
            get
            {
                if (!Цена.HasValue || !ДействующаяСкидка.HasValue || ДействующаяСкидка.Value <= 0)
                    return null;


                return Цена.Value - (Цена.Value * ДействующаяСкидка.Value / 100m);
            }
        }

        public string ПолныйПутьКФото
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Фото))
                    return "/Assets/picture.png"; 

                return $"/Assets/{Фото}";
            }
        }
        public bool ЕстьСкидка
        {
            get
            {
                return ДействующаяСкидка != null && ДействующаяСкидка > 0;
            }
        }

        public bool СкидкаБольше15
        {
            get
            {
                return ДействующаяСкидка != null && ДействующаяСкидка > 15;
            }
        }
    }
}