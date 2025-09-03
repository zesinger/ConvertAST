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
        //int Parse060(byte[] data, int offset)
        //{
        //    return (data[offset] & 0x80) != 0 ? 2 : 1;
        //}
        //int Parse245(byte[] data, int offset)
        //{
        //    int cursor = offset;
        //    while ((data[cursor] & 0x01) != 0)
        //        cursor++;
        //    return cursor - offset + 1;
        //}
        int Parse080(byte[] data, int offset)
        {
            // pour le 080, on compte le nombre d'octets jusqu'à trouver un octet avec le bit de continuation à 0
            int numBytes = 1;
            while ((data[offset] & 1) > 0)
            {
                numBytes++;
                offset++;
            }
            return numBytes;
        }
        // pour le I062/290, on compte le nombre d'octets jusqu'à trouver un octet avec le bit de continuation à 0
        // et chaque subfield présent ajoute sa longueur
        Dictionary<int, int> subfieldLength290 = new Dictionary<int, int>
        {
            [0] = 1,
            [1] = 1,
            [2] = 1,
            [3] = 1,
            [4] = 2,
            [5] = 1,
            [6] = 1,
            [7] = 1,
            [8] = 1,
            [9] = 1,
        };
        int Parse290(byte[] data, int offset)
        {
            int numBytes = 0;
            int addbit = 0;
            do
            {
                numBytes++;
                for (int i = 0; i < 7; i++)
                {
                    if ((data[offset] & (1 << (7 - i))) != 0)
                    {
                        if (subfieldLength290.TryGetValue(i + addbit, out int len))
                            numBytes += len;
                    }
                }
                addbit += 7;
                offset++;
            }
            while ((data[offset] & 1) > 0);
            return numBytes;
        }
        // pour le I062/295, on compte le nombre d'octets jusqu'à trouver un octet avec le bit de continuation à 0
        // et chaque subfield présent ajoute sa longueur qui est tout le temps 1
        int Parse295(byte[] data, int offset)
        {
            int numBytes = 0;
            int addbit = 0;
            do
            {
                numBytes++;
                for (int i = 0; i < 7; i++)
                {
                    if ((data[offset] & (1 << (7 - i))) != 0) numBytes++; // chaque subfield fait 1 octet
                }
                addbit += 7;
                offset++;
            }
            while ((data[offset] & 1) > 0);
            return numBytes;
        }

        // pour le I062/380, on compte le nombre d'octets jusqu'à trouver un octet avec le bit de continuation à 0
        // et chaque subfield présent ajoute sa longueur
        Dictionary<int, int> subfieldLength380 = new Dictionary<int, int>
        {
            [0] = 3,
            [1] = 6,
            [2] = 2,
            [3] = 2,
            [4] = 2,
            [5] = 2,
            [6] = 2,
            [7] = 1,
            [8] = 16,
            [9] = 2,
            [10] = 2,
            [11] = 7,
            [12] = 2,
            [13] = 2,
            [14] = 2,
            [15] = 2,
            [16] = 2,
            [17] = 2,
            [18] = 1,
            [19] = 8,
            [20] = 1,
            [21] = 6,
            [22] = 2,
            [23] = 1,
            [24] = 9,
            [25] = 2,
            [26] = 2,
            [27] = 2,
        };
        int Parse380(byte[] data, int offset)
        {
            int numBytes = 0;
            int addbit = 0;
            do
            {
                numBytes++;
                for (int i = 0; i < 7; i++)
                {
                    if ((data[offset] & (1 << (7 - i))) != 0)
                    {
                        if (subfieldLength380.TryGetValue(i + addbit, out int len))
                            numBytes += len;
                    }
                }
                addbit += 7;
                offset++;
            }
            while ((data[offset] & 1) > 0);
            return numBytes;
        }

        Dictionary<int, int> itemLength = new Dictionary<int, int>
        {
            [0] = 2,   // I062/010
            [1] = 0,   // spare
            [2] = 1,   // I062/015
            [3] = 3,   // I062/070
            [4] = 8,   // I062/105
            [5] = 6,   // I062/100
            [6] = 4,   // I062/185
            [7] = 2,  // I062/210
            [8] = 2,  // I062/060
            [9] = 7,  // I062/245
            [10] = -1,  // I062/380
            [12] = 2,  // I062/040
            [13] = -1,  // I062/080
            [14] = -1,  // I062/290
            [15] = 1,  // I062/200
            [16] = -1,  // I062/295
            [17] = 2,  // I062/136
            [18] = 2,  // I062/130
            [19] = 2,  // I062/135
            [20] = 2,  // I062/220
            [21] = -1,  // I062/390
            [22] = -1,  // I062/270
            [23] = 1,  // I062/300
            [24] = -1,  // I062/110
            [25] = 2,  // I062/120
            [26] = -1,  // I062/510
            [27] = -1,  // I062/500
            [28] = -1,  // I062/340
        };

        int ParseVariableLengthField(byte[] data, int offset, int fspecBit)
        {
            return fspecBit switch
            {
                10 => Parse380(data, offset),
                13 => Parse080(data, offset),
                14 => Parse290(data, offset),
                16 => Parse295(data, offset),
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
                    var filePath = saveFileDialog.FileName;

                    string cheminFichier = saveFileDialog.FileName; using var inputStream = new MemoryStream(_asterixData);
                    using var outputStream = new MemoryStream();
                    using var reader = new BinaryReader(inputStream);

                    pbJob.Maximum = (int)reader.BaseStream.Length;

                    while (reader.BaseStream.Position + 3 <= reader.BaseStream.Length)
                    {
                        long recordStart = reader.BaseStream.Position;
                        pbJob.Value = (int)recordStart;

                        byte category = reader.ReadByte();
                        //if (category!=1 && category !=4 && category!=62 && category!=65)
                        //{
                        //    int sqdf = 12;
                        //}
                        byte lenHi = reader.ReadByte();
                        byte lenLo = reader.ReadByte();
                        ushort length = (ushort)((lenHi << 8) | lenLo);

                        reader.BaseStream.Position = recordStart;
                        if (recordStart>500000)
                        {
                            int sdgsqd = 12;
                        }
                        byte[] record = reader.ReadBytes(length);

                        if (category != 062)
                        {
                            // Not CAT062 → copy the block unchanged
                            outputStream.Write(record, 0, record.Length);
                            continue;
                        }

                        // CAT062 block → try to replace I062/135 with I062/136
                        // https://www.eurocontrol.int/sites/default/files/2025-06/asterix-cat062-system-track-data-p9-ed1-21.pdf

                        /// structure des données CAT 062:
                        /// offset - taille de la donnée : description
                        /// 0000 - 1 octet  : CAT = 062
                        /// 0001 - 2 octets : LEN = longueur du bloc de données CAT 062 depuis l'offset 0000
                        /// 0003 - nFSPEC octets : FSPEC (field specifications) décrit les champs présents dans ce bloc de données
                        ///     la longueur du champ FSPEC n'est pas fixe, le bit le moins significatif de chaque octet contient
                        ///     0 si c'est le dernier octet du champ, 1 s'il y a encore au moins un octet pour le champ
                        ///     les 7 bits précédents signalent si les items correspondants sont présents
                        /// A partir de 0003+nFSPEC, on décrit tous les items s'ils ont été signalés présents dans FSPEC
                        /// La correspondance bit de FSPEC/items disponible est donné par la "Table 1 – System Track Data UAP"
                        /// dans le chapitre 5.3 :
                        /// bit 7 du premier octet de FSPEC : I062/010
                        /// bit 6 du premier octet de FSPEC : inutilisé
                        /// bit 5 du premier octet de FSPEC : I062/015
                        /// bit 4 du premier octet de FSPEC : I062/070
                        /// bit 3 du premier octet de FSPEC : I062/105
                        /// bit 2 du premier octet de FSPEC : I062/100
                        /// bit 1 du premier octet de FSPEC : I062/185
                        /// bit 0 du premier octet de FSPEC : bit de continuation
                        /// bit 7 du second octet de FSPEC : I062/210
                        /// bit 6 du second octet de FSPEC : I062/060
                        /// bit 5 du second octet de FSPEC : I062/245
                        /// bit 4 du second octet de FSPEC : I062/380
                        /// bit 3 du second octet de FSPEC : I062/040
                        /// bit 2 du second octet de FSPEC : I062/080
                        /// bit 1 du second octet de FSPEC : I062/290
                        /// bit 0 du second octet de FSPEC : bit de continuation
                        /// bit 7 du troisième octet de FSPEC : I062/200
                        /// bit 6 du troisième octet de FSPEC : I062/295
                        /// bit 5 du troisième octet de FSPEC : I062/136 <-- on remplace I062/135 par I062/136
                        /// bit 4 du troisième octet de FSPEC : I062/130
                        /// bit 3 du troisième octet de FSPEC : I062/135 <-- on remplace I062/135 par I062/136
                        /// ...

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

                        int bit135 = 18;
                        int bit136 = 16;

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
                            else if (i <= bit135 || i <= bit136)
                            {
                                itemLength.TryGetValue(i, out int itemLen);
                                if (itemLen >= 0)
                                {
                                    cursor += itemLen;
                                }
                                else
                                {
                                    // Variable-length field
                                    cursor += ParseVariableLengthField(record, cursor, i);
                                }
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

