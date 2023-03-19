using libc.serial.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libc.serial.AtDevice
{
  public abstract class AtCommand
  {
    public const string Ok = "OK";
    public const string Error = "ERROR";
    private const int _updateDataTimeout = 100;

    protected AtCommand()
    {
      timeout = new DelayedTask(onTimeout, 1);
      Exceptions = new List<(AtCommandExceptionReasons reason, Exception exception)>();
      responses = new List<string>();
      State = AtCommandStates.None;
      buffer = new StringBuilder();
      statesTrack = new List<AtCommandStates>();

      updateThread = new RepeatedTask(() =>
      {
        try
        {
          OnDataUpdate(buffer.ToString());
        }
        catch (Exception e)
        {
          Exceptions.Add((AtCommandExceptionReasons.OnUpdate, e));
          finish(false, AtCommandStates.Exception);
        }
      }, _updateDataTimeout);
    }

    public string ExceptionsString
    {
      get
      {
        return Exceptions.Select(a => $"Reason: {a.Item1} - Exception: {a.Item2}{Environment.NewLine}")
            .ConcatString(", ", "(", ")");
      }
    }

    public List<(AtCommandExceptionReasons reason, Exception exception)> Exceptions { get; }

    /// <summary>
    ///     Gets a copy of all states travered by this command
    /// </summary>
    public List<AtCommandStates> StatesTrack => new List<AtCommandStates>(statesTrack);

    /// <summary>
    ///     True if the command was successfully run by the device
    /// </summary>
    public bool IsSuccessful { get; private set; }

    /// <summary>
    ///     State of command
    /// </summary>
    public AtCommandStates State { get; private set; }

    /// <summary>
    ///     Gets a copy of all responses received from device
    /// </summary>
    public List<string> Responses => new List<string>(responses);

    /// <summary>
    ///     Is called when te operation is complete
    /// </summary>
    public event Action<AtCommand> Done;

    public static string LogError(AtCommand atCommand)
    {
      return string.Format(
          "Reading sms was not successful: {0}ExceptionsString: {1}{0}Responses: {2}{0}StatesTrack: {3}",
          Environment.NewLine, atCommand.ExceptionsString,
          atCommand.Responses.ConcatString(", ", "(", ")"),
          atCommand.StatesTrack.ConcatString(", ", "(", ")"));
    }

    /// <summary>
    ///     Control method ( non-blocking )
    /// </summary>
    /// <param name="port">Port.</param>
    /// <param name="onControlEnded">On control ended.</param>
    internal void ControlAsync(IComPort port, Action<AtCommand> onControlEnded)
    {
      this.port = port;
      onFinished = onControlEnded;
      setState(AtCommandStates.Started);
      updateThread.Start();

      try
      {
        RunCommandAsync();
      }
      catch (Exception e)
      {
        Exceptions.Add((AtCommandExceptionReasons.OnControl, e));
        finish(false, AtCommandStates.Exception);
      }
    }

    /// <summary>
    ///     Takes the buffer after something is received from device
    /// </summary>
    /// <param name="newData"></param>
    /// <returns></returns>
    internal void NewData(char newData)
    {
      buffer.Append(newData);
    }

    #region Protected

    /// <summary>
    ///     Add a response to the list of responses
    /// </summary>
    /// <param name="response"></param>
    protected void AddResponse(string response)
    {
      responses.Add(response);
    }

    /// <summary>
    ///     Called after a new char is arrived.
    /// </summary>
    protected abstract void OnDataUpdate(string buffer);

    protected abstract void RunCommandAsync();

    protected void setTimeout(int interval)
    {
      cancelTimeout();
      timeout.Duration = interval;
      timeout.Start();
    }

    protected void cancelTimeout()
    {
      timeout.Stop();
    }

    protected void finish(bool isSuccessful, AtCommandStates state)
    {
      cancelTimeout();
      updateThread.Stop();
      IsSuccessful = isSuccessful;
      setState(state);
      onFinished.BeginInvoke(this, null, null);
      Done?.BeginInvoke(this, null, null);
    }

    protected IComPort port;

    #endregion

    #region Private

    private readonly RepeatedTask updateThread;
    private readonly List<AtCommandStates> statesTrack;
    private readonly StringBuilder buffer;
    private readonly List<string> responses;
    private Action<AtCommand> onFinished;
    private readonly DelayedTask timeout;

    private void setState(AtCommandStates state)
    {
      State = state;
      statesTrack.Add(state);
    }

    private void onTimeout()
    {
      finish(false, AtCommandStates.Timedout);
    }

    #endregion
  }
}