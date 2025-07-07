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
        int Parse060(byte[] data, int offset)
        {
            return (data[offset] & 0x80) != 0 ? 2 : 1;
        }
        int Parse245(byte[] data, int offset)
        {
            int cursor = offset;
            while ((data[cursor] & 0x01) != 0)
                cursor++;
            return cursor - offset + 1;
        }
        int Parse380(byte[] data, int offset)
        {
            int cursor = offset;
            do
            {
                byte b = data[cursor++];
                if ((b & 0x01) == 0)
                    break;
            } while (cursor < data.Length);
            return cursor - offset;
        }
        Dictionary<int, int> fixedLengthMap = new Dictionary<int, int>
        {
            [0] = 2,   // I062/010
            [1] = 1,   // I062/015
            [2] = 2,   // I062/070
            [3] = 6,   // I062/105
            [4] = 4,   // I062/100
            [5] = 2,   // I062/185
            [6] = 2,   // I062/210
            [10] = 1,  // I062/040
            [11] = 2,  // I062/080
            [12] = 2,  // I062/200
            [13] = 2,  // I062/135
            [14] = 2,  // I062/136
        };

        int ParseVariableLengthField(byte[] data, int offset, int fspecBit)
        {
            return fspecBit switch
            {
                7 => (data[offset] & 0x80) != 0 ? 2 : 1, // I062/060
                8 => Parse245(data, offset),            // I062/245
                9 => Parse380(data, offset),            // I062/380
                _ => throw new NotSupportedException($"Unsupported variable-length field for FSPEC bit {fspecBit}")
            };
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
                        if (reader.BaseStream.Position>520000)
                        {
                            int fdh = 12;
                        }
                        long recordStart = reader.BaseStream.Position;

                        byte category = reader.ReadByte();
                        byte lenHi = reader.ReadByte();
                        byte lenLo = reader.ReadByte();
                        ushort length = (ushort)((lenHi << 8) | lenLo);

                        reader.BaseStream.Position = recordStart;
                        byte[] record = reader.ReadBytes(length);

                        if (category != 062)
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

                        bool seen136 = false;
                        bool seen135 = false;

                        for (int i = 0; i < fspecBits.Count && cursor < record.Length; i++)
                        {
                            if (!fspecBits[i])
                                continue;

                            if (i == bit135)
                            {
                                offset135 = cursor;
                                cursor += 2;
                                seen135 = true;
                            }
                            else if (i == bit136)
                            {
                                offset136 = cursor;
                                cursor += 2;
                                seen136 = true;
                            }
                            else if (fixedLengthMap.TryGetValue(i, out int len))
                            {
                                cursor += len;
                            }
                            else if (!seen136 || !seen135)
                            {
                                // Need to parse variable-length field properly
                                // You can plug in a per-field handler here, e.g.:
                                cursor += ParseVariableLengthField(record, cursor, i);
                            }
                            else
                            {
                                // We’ve passed 135 and 136, just copy the rest
                                break;
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

