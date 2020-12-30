using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Tar;

namespace TestContainers.Utility
{
    public interface ITransferable
    {
        int FileMode { get; }
        long Size { get; }

        byte[] GetBytes();

        string Description { get; }
    }

    public class Transferable : ITransferable
    {
        public static readonly int DEFAULT_FILE_MODE = 0100644;
        public static readonly int DEFAULT_DIR_MODE = 040755;
        private byte[] _bytes;
        private int _fileMode;

        private Transferable(byte[] bytes, int fileMode)
        {
            _bytes = bytes;
            _fileMode = fileMode;
        }

        public static Transferable Of(byte[] bytes)
        {
            return Of(bytes, DEFAULT_FILE_MODE);
        }

        public static Transferable Of(byte[] bytes, int fileMode)
        {
            return new Transferable(bytes, fileMode);
        }

        //public Task TransferTo(TarArchive tarArchive, string destination)
        //{
        //    var tarEntry = TarEntry.CreateTarEntry(destination);// new TarEntry();
        //    tarEntry.Size=Size;
        //    tarEntry.TarHeader.Mode=FileMode;

        //    try
        //    {
        //        tarArchive.WriteEntry(tarEntry, false);
        //        tarArchiveOutputStream.putArchiveEntry(tarEntry);
        //        IOUtils.write(getBytes(), tarArchiveOutputStream);
        //        tarArchiveOutputStream.closeArchiveEntry();
        //    }
        //    catch (IOException e)
        //    {
        //        throw new RuntimeException("Can't transfer " + getDescription(), e);
        //    }
        //}


        public virtual int FileMode => _fileMode;

        public virtual long Size => _bytes.Length;

        public virtual string Description => "";

        public virtual byte[] GetBytes() => _bytes;
    }
}
