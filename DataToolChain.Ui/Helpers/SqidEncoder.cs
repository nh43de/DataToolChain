//using Sqids;

//namespace DataToolChain.Ui.Helpers;

//public class SqidEncoder
//{
//    public SqidEncoder(string alphabet) 
//    {
//        _sqidEncoder = new(new SqidsOptions()
//        {
//            MinLength = 0,
//            Alphabet = alphabet
//        });
//    }

//    private readonly SqidsEncoder<int> _sqidEncoder;

//    public string EncodeInt(int i)
//    {
//        var r = _sqidEncoder.Encode(i);

//        return r;
//    }

//    public int? DecodeInt(string? incomingId)
//    {
//        if (incomingId == null)
//            return null;

//        //use canonical
//        if (_sqidEncoder.Decode(incomingId) is [var decodedId] &&
//            incomingId == _sqidEncoder.Encode(decodedId))
//        {
//            return decodedId;
//            // `incomingId` decodes into a single number and is canonical, here you can safely proceed with the rest of the logic
//        }
//        else
//        {
//            return null;
//            // consider `incomingId` invalid — e.g. respond with 404
//        }
//    }
//}