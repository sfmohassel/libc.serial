using libc.serial.AtDevice;
using libc.serial.Internal;
using System.Collections.Generic;

namespace libc.serial.Sim900
{
  public class AtCGetCharge : AtCommand
  {
    private static readonly IDictionary<SimCardOperators, string> _getCharge = new Dictionary
        <SimCardOperators, string>
            {
                {
                    SimCardOperators.Irancell, $"at+cusd=1,\"{"*141*1#"}\"\r"
                },
                {
                    SimCardOperators.HamrahAval, $"at+cusd=1,\"{"*140*11#"}\"\r"
                }
            };

    private readonly string _cmd;

    public AtCGetCharge(SimCardOperators simOperator)
    {
      SimCardOperator = simOperator;
      Etebar = -1;
      _cmd = _getCharge[simOperator];
    }

    public int Etebar { get; private set; }
    public SimCardOperators SimCardOperator { get; }

    public override string ToString()
    {
      return $"Command: {_cmd} - State: {State} - Etebar: {Etebar}";
    }

    protected override void RunCommandAsync()
    {
      port.Write(_cmd);
      setTimeout(6000);
    }

    protected override void OnDataUpdate(string buffer)
    {
      buffer = buffer.StartingFrom(_cmd);

      if (buffer == null) return;

      if (buffer.Contains(Error))
      {
        AddResponse(Error);
        finish(false, AtCommandStates.Completed);
      }
      else if (buffer.Contains(Ok))
      {
        var bCusd = buffer.GetBetween("+CUSD:", "\r\n");

        if (bCusd != null && bCusd.Contains("Etebar:") && bCusd.Contains("Rial"))
        {
          Etebar = getEtebar(bCusd);
          AddResponse(Ok);
          finish(true, AtCommandStates.Completed);
        }
      }
    }

    private int getEtebar(string s)
    {
      var etebar = -1;
      var m = s.GetBetween("Etebar:", "Rial");

      if (m != null)
      {
        int.TryParse(m.Trim(), out etebar);

        return etebar;
      }

      return etebar;
    }
  }
}