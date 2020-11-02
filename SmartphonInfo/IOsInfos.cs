using System;
using System.Collections.Generic;
using System.Text;

namespace SmartphonInfo {
   public interface IOsInfos {

      string[] InfoGroupNames();

      string Info();

      string Info(int group);

   }
}
