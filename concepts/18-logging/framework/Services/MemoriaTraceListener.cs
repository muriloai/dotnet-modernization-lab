using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using LoggingDemo.Models;

namespace LoggingDemo.Services
{
    public class MemoriaTraceListener : TraceListener
    {
        private static readonly ConcurrentQueue<LogLegadoItem> _historico = new ConcurrentQueue<LogLegadoItem>();
        private const int LimiteHistorico = 100;

        public MemoriaTraceListener() : base("MemoriaTraceListener")
        {
        }

        public MemoriaTraceListener(string name) : base(name)
        {
        }

        public static IReadOnlyList<LogLegadoItem> ObterHistorico()
        {
            return _historico.Reverse().ToList();
        }

        public static void LimparHistorico()
        {
            LogLegadoItem descartado;
            while (_historico.TryDequeue(out descartado)) { }
        }

        private static void Enfileirar(string tipo, string mensagem)
        {
            _historico.Enqueue(new LogLegadoItem(tipo, mensagem));
            LogLegadoItem removido;
            while (_historico.Count > LimiteHistorico && _historico.TryDequeue(out removido)) { }
        }

        public override void Write(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                Enfileirar("Trace", message);
            }
        }

        public override void WriteLine(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                Enfileirar("Trace", message);
            }
        }

        public override void TraceEvent(
            TraceEventCache eventCache,
            string source,
            TraceEventType eventType,
            int id,
            string message)
        {
            Enfileirar(eventType.ToString(), message);
        }

        public override void TraceEvent(
            TraceEventCache eventCache,
            string source,
            TraceEventType eventType,
            int id,
            string format,
            params object[] args)
        {
            var formatada = (args != null && args.Length > 0) ? string.Format(format, args) : format;
            Enfileirar(eventType.ToString(), formatada);
        }
    }
}
