using System.IO;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Tar;

namespace TestContainers.Utility
{
    public abstract class AbstractTransferable : ITransferable
    {
        internal static int DEFAULT_FILE_MODE = 0100644;
        internal static int DEFAULT_DIR_MODE = 040755;



        public abstract int FileMode { get; }

        public abstract long Size { get; }

        public abstract string Description { get; }

        public abstract byte[] GetBytes();

        /**
     * transfer content of this Transferable to the output stream. <b>Must not</b> close the stream.
     *
     * @param tarArchiveOutputStream stream to output
     * @param destination
     */
        public virtual async Task TransferTo(TarOutputStream tarArchiveOutputStream, string destination)
        {
            var tarEntry = TarEntry.CreateTarEntry(destination);
            tarEntry.Size = Size;
            tarEntry.TarHeader.Mode = FileMode;

            try
            {
                tarArchiveOutputStream.PutNextEntry(tarEntry);
                //IOUtils.write(getBytes(), tarArchiveOutputStream);
                tarArchiveOutputStream.CloseEntry();
            }
            catch (IOException e)
            {
                throw e;// new RuntimeException("Can't transfer " + getDescription(), e);
            }
        }

    }
}
