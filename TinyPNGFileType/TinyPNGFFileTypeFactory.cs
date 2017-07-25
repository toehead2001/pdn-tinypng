using PaintDotNet;

namespace TinyPNGPlugin
{
  public class TinyPNGFileTypeFactory : IFileTypeFactory
  {
    public FileType[] GetFileTypeInstances()
    {
      return new FileType[]
      {
         new TinyPNGFileType()
      };
    }
  }
}
