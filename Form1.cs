using static System.Runtime.InteropServices.JavaScript.JSType;

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

        Dictionary<int, (int, int)> itemLength = new Dictionary<int, (int, int)>
        {
            [0] = (2,10),   // I062/010
            [1] = (0,0),   // spare
            [2] = (1,15),   // I062/015
            [3] = (3,70),   // I062/070
            [4] = (8,105),   // I062/105
            [5] = (6,100),   // I062/100
            [6] = (4,185),   // I062/185
            [7] = (2,210),  // I062/210
            [8] = (2,60),  // I062/060
            [9] = (7,245),  // I062/245
            [10] = (-1,380),  // I062/380
            [11] = (2,40),  // I062/040
            [12] = (-1,80),  // I062/080
            [13] = (-1,290),  // I062/290
            [14] = (1,200),  // I062/200
            [15] = (-1,295),  // I062/295
            [16] = (2,136),  // I062/136
            [17] = (2,130),  // I062/130
            [18] = (2,135),  // I062/135
            [19] = (2,220),  // I062/220
            [20] = (-1,390),  // I062/390
            [21] = (-1,270),  // I062/270
            [22] = (1,300),  // I062/300
            [23] = (-1,110),  // I062/110
            [24] = (2,120),  // I062/120
            [25] = (-1,510),  // I062/510
            [26] = (-1,500),  // I062/500
            [27] = (-1,340),  // I062/340
        };

        void ToDms(double val, out int dd, out int mm, out int ss)
        {
            double absV = Math.Abs(val);
            dd = (int)Math.Floor(absV);
            mm = (int)Math.Floor((absV - dd) * 60.0);
            ss = (int)Math.Round(((absV - dd) * 60.0 - mm) * 60.0);
            if (ss == 60) { ss = 0; mm++; }
            if (mm == 60) { mm = 0; dd++; }
            if (val < 0) dd = -dd;
        }

        string DescribeItemData((int Len, int Id) item, byte[] content)
        {
            switch (item.Id)
            {
                case 40: // I062/040
                    if (content.Length == 2)
                    {
                        int val = (content[0] << 8) | content[1];
                        return $" Numéro de piste : {val}\r\n";
                    }
                    break;
                case 70: // I062/070
                 if (content.Length == 3)
                    {
                        int T = (content[0] << 16) | (content[1] << 8) | content[2];      // 24 bits
                        int totalSeconds = (T + 64) / 128;        // +64 pour arrondir (≈ +0,5s)

                        // on borne à 0..86399 au cas où
                        totalSeconds %= 24 * 3600;

                        int h = totalSeconds / 3600;
                        int m = (totalSeconds % 3600) / 60;
                        int s = totalSeconds % 60;

                        string hhmmss = $"{h:00}:{m:00}:{s:00}";  // en UTC                        return $"      Time of day : {val} ms\r\n";
                        return $" Heure : {hhmmss} UTC\r\n";
                    }
                    break;
                case 105: // I062/105
                    if (content.Length == 8)
                    {
                        int rawLat = (content[0] << 24) | (content[1] << 16) | (content[2] << 8) | content[3];
                        int rawLon = (content[4] << 24) | (content[5] << 16) | (content[6] << 8) | content[7];

                        const double LSB = 180.0 / (1 << 25);

                        double latitude = rawLat * LSB;
                        double longitude = rawLon * LSB;
                        ToDms(latitude, out int latDeg, out int latMin, out int latSec);
                        string latStr = $"{(latDeg < 0 ? "-" : "+")}{Math.Abs(latDeg):00}°{latMin:00}'{latSec:00}\"";

                        ToDms(longitude, out int lonDeg, out int lonMin, out int lonSec);
                        string lonStr = $"{(lonDeg < 0 ? "-" : "+")}{Math.Abs(lonDeg):000}°{lonMin:00}'{lonSec:00}\"";

                        return $" Position : Lat {latStr}, Lon {lonStr}\r\n";
                    }
                    break;
                case 135: // I062/135
                    if (content.Length == 2)
                    {
                        short raw = (short)((content[0] << 8) | content[1]);

                        bool qnhApplied = (raw & 0x8000) != 0;
                        short value = (short)(raw & 0x7FFF); // altitude en quart de FL

                        double altitudeFeet = value * 25.0;
                        double flightLevel = altitudeFeet / 100.0;
                        return $" Altitude : {flightLevel} ft\r\n";
                    }
                    break;
                case 136: // I062/136
                    if (content.Length == 2)
                    {
                        short raw = (short)((content[0] << 8) | content[1]);

                        double altitudeFeet = raw * 25.0;
                        double flightLevel = altitudeFeet / 100.0;
                        if (flightLevel < 0)
                            return $" Altitude par défaut, à ignorer.\r\n";
                        else return $" Altitude : {flightLevel} ft\r\n";
                    }
                    break;
                case 245: // I062/245
                    if (content.Length == 7)
                    {
                        char[] ia5Table = new char[]
                        {   ' ', 'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O',
                            'P','Q','R','S','T','U','V','W','X','Y','Z','0','1','2','3','4','5','6','7','8','9'
                        };
                        byte[] bytes = content.Skip(1).Take(6).ToArray();
                        // concaténer les 6 octets en un ulong
                        ulong value = 0;
                        for (int i = 0; i < 6; i++)
                            value = (value << 8) | bytes[i];

                        char[] result = new char[8];
                        for (int i = 7; i >= 0; i--)
                        {
                            int code = (int)(value & 0x3F); // 6 bits
                            value >>= 6;
                            result[i] = (code < ia5Table.Length) ? ia5Table[code] : ' ';
                        }
                        return $" Callsign ou immatriculation : '{new string(result).Trim()}'\r\n";
                    }
                    break;
                default: return "\r\n";
            }
            return "\r\n";
        }
        (int itemLen, int itemId) ParseVariableLengthField(byte[] data, int offset, int fspecBit)
        {
            return fspecBit switch
            {
                10 => (Parse380(data, offset), 380),
                12 => (Parse080(data, offset), 080),
                13 => (Parse290(data, offset), 290),
                15 => (Parse295(data, offset), 295),
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
                    tbLog.Text = string.Empty;
                    bool Success = true;
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

                        tbLog.AppendText($"\r\nBloc à l'offset {recordStart} du fichier, CAT{category:D3}, longueur {length:D6}");
                        if (category==062) tbLog.AppendText(" :\r\n");
                        else tbLog.AppendText("\r\n");

                        if (length < 3 || reader.BaseStream.Position - 3 + length > reader.BaseStream.Length)
                        {
                            // Invalid length, stop processing
                            tbLog.AppendText("\r\nLongueur invalide, arrêt du process.");
                            Success = false;
                            break;
                        }

                        reader.BaseStream.Position = recordStart;

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

                        string log = "";
                        byte[] content;
                        for (int i = 0; i < fspecBits.Count && cursor < record.Length; i++)
                        {
                            if (!fspecBits[i])
                                continue;

                            if (i == bit135)
                            {
                                offset135 = cursor;
                                log += $"   Item I062/135 trouvé à l'offset {cursor:D6}.";
                                content=record.AsSpan(cursor,2).ToArray();
                                log += DescribeItemData((2,135), content); 
                                cursor += 2;
                            }
                            else if (i == bit136)
                            {
                                offset136 = cursor;
                                log+=$"   Item I062/136 trouvé à l'offset {cursor:D6}.";
                                content = record.AsSpan(cursor, 2).ToArray();
                                log += DescribeItemData((2,136), content);
                                cursor += 2;
                            }
                            else if (i <= bit135 || i <= bit136)
                            {
                                (int itemLen, int itemId) valres;
                                itemLength.TryGetValue(i, out valres);
                                if (valres.itemLen >= 0)
                                {
                                    log += $"   Item I062/{valres.itemId:D3} trouvé à l'offset {cursor:D6}.";
                                    content = record.AsSpan(cursor, valres.itemLen).ToArray();
                                    log += DescribeItemData(valres, content);
                                    cursor += valres.itemLen;
                                }
                                else
                                {
                                    // Variable-length field
                                    (int itemLen, int itemId) itemres= ParseVariableLengthField(record, cursor, i);
                                    log+=$"   Item I062/{itemres.itemId:D3} trouvé à l'offset {cursor:D6}.";
                                    content = record.AsSpan(cursor, Math.Min(itemres.itemLen, record.Length-cursor)).ToArray();
                                    log += DescribeItemData(valres, content);
                                    cursor += itemres.itemLen;
                                }
                            }
                            else
                            {
                                // We’ve passed 135 and 136, just copy the rest
                                log+="   Position des items I062/135 et I062/136 dépassé, on copie juste le contenu restant.\r\n";
                                break;
                            }
                        }
                        // Perform the replacement if both fields are found
                        if (offset135.HasValue && offset136.HasValue)
                        {
                            if (record[offset136.Value] == 255 && record[offset136.Value + 1] == 255)
                            {
                                record[offset136.Value] = (byte)(record[offset135.Value] & 0x7f);
                                record[offset136.Value + 1] = record[offset135.Value + 1];
                                log += "   On a trouvé les 2 items I062/135 et I062/136, mais la valeur du I062/136 est invalide, on remplace donc la valeur de l'item 136 par celle de l'item 135.\r\n";
                            }
                            else
                            {
                                record[offset135.Value] = (byte)(record[offset136.Value] & 0x7f);
                                record[offset135.Value + 1] = record[offset136.Value + 1];
                                log += "   On a trouvé les 2 items I062/135 et I062/136, on remplace donc la valeur de l'item 135 par celle de l'item 136.\r\n";
                            }
                        }
                        tbLog.AppendText(log);
                        outputStream.Write(record, 0, record.Length);
                    }
                    _outputData = outputStream.ToArray();
                    using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                    {
                        fs.Write(_outputData, 0, _outputData.Length);
                    }
                    bSave.Enabled = false;
                    if (Success) MessageBox.Show("Fichier sauvegardé.", "Succés", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    else MessageBox.Show("Le fichier a été sauvegardé, mais un bloc avec une longueur erronnée a été trouvée pendant le traitement, le résultat est donc incomplet.", "Attention", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            //catch
            //{
            //    MessageBox.Show("An error occurred while processing the file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //}
        }
    }
}

