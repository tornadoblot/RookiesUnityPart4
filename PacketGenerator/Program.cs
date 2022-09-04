using System;
using System.Xml;

namespace PacketGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            XmlReaderSettings settings = new XmlReaderSettings()
            {
                // 주석과 공백 무시
                IgnoreComments = true,
                IgnoreWhitespace = true
            };

            // 원래 다 쓰면 dispose로 나가줘야 하는데 using 써서 자동으로 해줌
            using (XmlReader r = XmlReader.Create("PDL.xml", settings))
            {
                r.MoveToContent();

                while(r.Read())
                {
                    // nodetype element는 태그의 시작 위치, EndElement는 끝나는 위치
                    if (r.Depth == 1 && r.NodeType == XmlNodeType.Element)
                        ParsePacket(r);

                    // 타입은 r.name, 어트리뷰트는 r["name"]
                    Console.WriteLine(r.Name + " " + r["name"]);
                }

            }

            
               
        }

        public static void ParsePacket(XmlReader r)
        {
            // 태그 끝이면 리턴
            if (r.NodeType == XmlNodeType.EndElement)
                return;

            // 소문자화 해서 패킷이 아니면 리턴
            if (r.Name.ToLower() != "packet")
            {
                Console.WriteLine("Invalid packet node");
                return;
            }

            string packetName = r["name"];
            if (string.IsNullOrEmpty(packetName))
            {
                Console.WriteLine("Packet without name");
                return;
            }

            ParseMembers(r);
        }


        public static void ParseMembers(XmlReader r)
        {
            string packetName = r["name"];

            // depth는 <packet> 부분
            int depth = r.Depth + 1;
            while(r.Read())
            {
                // packet 부분을 넘어가면 멈춤
                if (r.Depth != depth)
                    break;

                // name 속성이 없으면 리턴
                string memberName = r["name"];
                if(string.IsNullOrEmpty(memberName))
                {
                    Console.WriteLine("Member without name");
                    return;
                }

                string memberType = r.Name.ToLower();
                switch(memberType)
                {
                    case "bool":
                    case "byte":
                    case "short":
                    case "ushort":
                    case "int":
                    case "long":
                    case "float":
                    case "double":
                    case "string":
                    case "list":
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
