using System.Text;
using System.Xml.Serialization;
using libc.models;
using Newtonsoft.Json;
namespace libc.serial.RequestReponse {
    public abstract class ComRRResponse {
        protected ComRRResponse() {
            Validation = new FluentResult();
        }
        [JsonIgnore]
        [XmlIgnore]
        public FluentResult Validation { get; set; }
        public override string ToString() {
            var res = new StringBuilder();
            res.Append($"Valid: {Validation.Success}");
            res.Append("__");
            if (!Validation.Success) {
                res.Append($"Error: {Validation.ConcatErrors()}");
                res.Append("__");
            }
            String(res);
            return res.ToString();
        }
        protected abstract void String(StringBuilder res);
    }
}