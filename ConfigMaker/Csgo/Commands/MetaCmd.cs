using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigMaker.Csgo.Commands
{
    public class MetaCmd : CommandCollection
    {
        public AliasCmd AliasOnKeyDown { get; set; } = null;
        public AliasCmd AliasOnKeyRelease { get; set; } = null;

        public MetaCmd() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">Название мета-скрипта без префиксов + и -</param>
        /// <param name="onKeyDown">Набор команд при нажатии кнопки. </param>
        /// <param name="onKeyRelease">Набор команд при отжатии кнопки.</param>
        public MetaCmd(string name, CommandCollection onKeyDown, CommandCollection onKeyRelease)
        {
            //int onDownCount = onKeyDown != null ? onKeyDown.Count : 0;
            //int onReleaseCount = onKeyRelease != null ? onKeyRelease.Count : 0;
            //int totalCmdCount = onDownCount + onReleaseCount;

            //if (totalCmdCount == 0)
            //    throw new InvalidOperationException("В мета-скрипте нет ни одной команды");

            //bool needPrefix = onDownCount > 0 && onReleaseCount > 0;
            //if (onKeyDown == null && onKeyRelease == null)
            //    throw new InvalidOperationException("Обе коллекции равны null");

            //int nullCount = 0;
            //if (onKeyDown == null) nullCount++;
            //if (onKeyRelease == null) nullCount++;
            if (onKeyDown == null || onKeyRelease == null)
            {
                throw new InvalidOperationException("Обе коллекции не могут быть равны null");
            }

            //// Ставим префикс только если обе коллекции существуют (могут быть пустыми)
            //bool needPrefix = nullCount == 2;

            //if (onKeyDown != null && onKeyDown.Count > 0)
            //{
            this.AliasOnKeyDown = new AliasCmd($"+{name}", onKeyDown);
            this.Add(this.AliasOnKeyDown);
            //}

            //if (onKeyRelease != null && onKeyRelease.Count > 0)
            //{
            this.AliasOnKeyRelease = new AliasCmd($"-{name}", onKeyRelease);
            this.Add(this.AliasOnKeyRelease);
            //}
        } 
    }
}
