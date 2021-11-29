// -----------------------------------------------------------------------
// <copyright file="PresetHeartbeat.cs" company="QinetiQ">
// TODO: Update copyright text.
// Crown-Owned Copyright (c)
// </copyright>
// -----------------------------------------------------------------------

namespace SapientMiddleware
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using System.Text;
  using SapientServices;

  /// <summary>
  /// TODO: Update summary.
  /// </summary>
  public class PresetHeartbeat
  {
    private StatusReport statusReport;

    private SensorRegistration registration;

    public PresetHeartbeat()
    {
      statusReport = new StatusReport();
      registration = new SensorRegistration();
    }

    public void LoadRegistration(string fileName)
    {
      try
      {
        Console.WriteLine("Opening Reg File:" + fileName + "\n");
        StreamReader sr = new StreamReader(new FileStream(fileName, FileMode.Open, FileAccess.Read));
        string message = sr.ReadToEnd();
      }
      catch ( FileNotFoundException )
      {
        Console.WriteLine("File Not Found: " + fileName);
      }
      catch ( Exception ex )
      {
        Console.WriteLine(ex.ToString());
      }
    }

  }
}
