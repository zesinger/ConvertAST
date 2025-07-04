namespace ConvertAST
{
    public partial class ConvertAST : Form
    {
        private string _filePath = string.Empty;
        private byte[] _asterixData = [];
        private byte[] _outputData = [];
        public ConvertAST()
        {
            InitializeComponent();
        }

        private void bLoad_Click(object sender, EventArgs e)
        {
            try
            {
                _filePath = string.Empty;

                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.InitialDirectory = "c:\\";
                    openFileDialog.Filter = "Fichier ASTERIX (*.ast)|*.ast";
                    openFileDialog.FilterIndex = 2;
                    openFileDialog.RestoreDirectory = true;

                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        bSave.Enabled = false;
                        // on obtient le chemin du fichier sélectionné
                        _filePath = openFileDialog.FileName;
                        var fileStream = openFileDialog.OpenFile();
                        using (BinaryReader reader = new BinaryReader(fileStream))
                        {
                            _asterixData = reader.ReadBytes((int)fileStream.Length);
                            bSave.Enabled = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void bSave_Click(object sender, EventArgs e)
        {
            //try
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Title = "Enregistrer un fichier",
                    Filter = "Fichiers ASTERIX (*.ast)|*.ast",
                    DefaultExt = "ast",
                    AddExtension = true,
                    FileName = _filePath
                };

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    var filePath= saveFileDialog.FileName;

                    string cheminFichier = saveFileDialog.FileName; using var inputStream = new MemoryStream(_asterixData);
                    using var outputStream = new MemoryStream();
                    using var reader = new BinaryReader(inputStream);

                    while (reader.BaseStream.Position + 3 <= reader.BaseStream.Length)
                    {
                        long recordStart = reader.BaseStream.Position;

                        byte category = reader.ReadByte();
                        byte lenHi = reader.ReadByte();
                        byte lenLo = reader.ReadByte();
                        ushort length = (ushort)((lenHi << 8) | lenLo);

                        reader.BaseStream.Position = recordStart;
                        byte[] record = reader.ReadBytes(length);

                        if (category != 0x3E)
                        {
                            // Not CAT062 → copy the block unchanged
                            outputStream.Write(record, 0, record.Length);
                            continue;
                        }

                        // CAT062 block → try to replace I062/135 with I062/136
                        int fspecStart = 3;
                        int fspecEnd = fspecStart;
                        List<bool> fspecBits = new();

                        bool moreFSPEC = true;
                        while (moreFSPEC && fspecEnd < record.Length)
                        {
                            byte b = record[fspecEnd++];
                            for (int i = 7; i >= 1; i--) fspecBits.Add((b & (1 << i)) != 0);
                            moreFSPEC = (b & 1) != 0;
                        }

                        int bit135 = 13;
                        int bit136 = 14;

                        int cursor = fspecEnd;
                        int? offset135 = null;
                        int? offset136 = null;

                        for (int i = 0; i < fspecBits.Count && cursor + 1 < record.Length; i++)
                        {
                            if (!fspecBits[i]) continue;

                            if (i == bit135)
                            {
                                offset135 = cursor;
                                cursor += 2;
                            }
                            else if (i == bit136)
                            {
                                offset136 = cursor;
                                cursor += 2;
                            }
                            else
                            {
                                // Unknown fixed-length field → skip 2 bytes (safe default)
                                cursor += 2;
                            }
                        }

                        // Perform the replacement if both fields are found
                        if (offset135.HasValue && offset136.HasValue)
                        {
                            record[offset135.Value] = record[offset136.Value];
                            record[offset135.Value + 1] = record[offset136.Value + 1];
                        }

                        outputStream.Write(record, 0, record.Length);
                    }
                    _outputData = outputStream.ToArray();
                    using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                    {
                        fs.Write(_outputData, 0, _outputData.Length);
                    }
                    bSave.Enabled = false;
                    MessageBox.Show("File saved successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            //catch
            //{
            //    MessageBox.Show("An error occurred while processing the file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //}
        }
    }
}

