using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleAppTwofish
{
    public class AlgorithmTwofish
    {
        static private byte[,] RS = new byte[,]
                                        {{0x01, 0xA4, 0x55, 0x87, 0x5A, 0x58, 0xDB, 0x9E},
                                        {0xA4, 0x56, 0x82, 0xF3, 0x1E, 0xC6, 0x68, 0xE5},
                                        {0x02, 0xA1, 0xFC, 0xC1, 0x47, 0xAE, 0x3D, 0x19},
                                        {0xA4, 0x55, 0x87, 0x5A, 0x58, 0xDB, 0x9E, 0x03}};
        static private byte[,] tq0 = new byte[,]
                                        {{0x8, 0x1, 0x7, 0xD, 0x6, 0xF, 0x3, 0x2, 0x0, 0xB, 0x5, 0x9, 0xE, 0xC, 0xA, 0x4},
                                        {0xE, 0xC, 0xB, 0x8, 0x1, 0x2, 0x3, 0x5, 0xF, 0x4, 0xA, 0x6, 0x7, 0x0, 0x9, 0xD},
                                        {0xB, 0xA, 0x5, 0xE, 0x6, 0xD, 0x9, 0x0, 0xC, 0x8, 0xF, 0x3, 0x2, 0x4, 0x7, 0x1},
                                        {0xD, 0x7, 0xF, 0x4, 0x1, 0x2, 0x6, 0xE, 0x9, 0xB, 0x3, 0x0, 0x8, 0x5, 0xC, 0xA}};
        static private byte[,] tq1 = new byte[,]
                                        {{0x2, 0x8, 0xB, 0xD, 0xF, 0x7, 0x6, 0xE, 0x3, 0x1, 0x9, 0x4, 0x0, 0xA, 0xC, 0x5},
                                        {0x1, 0xE, 0x2, 0xB, 0x4, 0xC, 0x3, 0x7, 0x6, 0xD, 0xA, 0x5, 0xF, 0x9, 0x0, 0x8},
                                        {0x4, 0xC, 0x7, 0x5, 0x1, 0x6, 0x9, 0xA, 0x0, 0xE, 0xD, 0x8, 0x2, 0xB, 0x3, 0xF},
                                        {0xB, 0x9, 0x5, 0x1, 0xC, 0x3, 0xD, 0xE, 0x6, 0x4, 0x7, 0xF, 0x2, 0x0, 0x8, 0xA}};

        static private List<byte[,]> tq = new List<byte[,]> { tq0, tq1 };

        static private byte[,] MDS = new byte[,]
                                        {{0x01, 0xEF, 0x5B, 0x5B},
                                        {0x5B, 0xEF, 0xEF, 0x01},
                                        {0xEF, 0x5B, 0x01, 0xEF},
                                        {0xEF, 0x01, 0xEF, 0x5B}};

        private int key_length;
        private byte[] key;
        private byte[][] k_keys;
        private byte[][] s_keys;

        public AlgorithmTwofish()
        {
            key_length = 128;
            key = null;
        }

        public void SetKey(int keylength, byte[] key)
        {
            if (keylength != 128)
                throw new Exception("For this version of the program,a key with a length of only 128 is supported.");
            if (key.Length * 8 == keylength)
            {
                this.key = key;
                this.key_length = keylength;
            }
            else
                throw new Exception("Key is not correct");
        }

        public byte[] Encrypt(byte[] plaintext)
        {
            List<byte> plaintext_list = plaintext.ToList();
            List<byte> ciphertext_list = new List<byte>();
            while (plaintext_list.Count%16 != 0)
                plaintext_list.Add(0);
            for(int i = 0; i < plaintext_list.Count; i += 16)
            {
                ciphertext_list.AddRange(Encrypt_block(plaintext_list.GetRange(i,16).ToArray()));
            }
            return ciphertext_list.ToArray();
        }

        public byte[] Decrypt(byte[] ciphertext)
        {
            List<byte> plaintext_list = new List<byte>();
            List<byte> ciphertext_list = ciphertext.ToList();
            while (ciphertext_list.Count % 16 != 0)
                ciphertext_list.Add(0);
            for (int i = 0; i < ciphertext_list.Count; i += 16)
            {
                plaintext_list.AddRange(Decrypt_block(ciphertext_list.GetRange(i, 16).ToArray()));
            }
            return plaintext_list.ToArray();
        }

        public byte[] Encrypt_block(byte[] plaintext)
        {
            if (plaintext.Length != 16)
                throw new Exception("Something go wrong! The block size is not equal to 128");
            if (key == null)
                throw new Exception("Key is null. Use SetKey method.");

            // Разделение открытого текста на 4 ветви
            byte[][] text_branches = new byte[4][];
            for (int i = 0; i < 4; i++)
            {
                text_branches[i] = new byte[4];
                for (int j = 0; j < 4; j++)
                {
                    text_branches[i][j] = plaintext[i * 4 + j];
                }
            }

            // KeyShedule - 
            KeyShedule(out k_keys, out s_keys, ref key);

            // Входное отбеливание
            byte[][] keys_input_whitening = new byte[][] { k_keys[0], k_keys[1], k_keys[2], k_keys[3] };
            Whitening(ref text_branches, ref keys_input_whitening);

            // 16 раундов сети Фейстеля
            for (int round = 0; round < 16; round++)
            {
                byte[][] f_result = f_function(text_branches[0], text_branches[1], round);

                byte[] c2 = XOR(f_result[0], text_branches[2]);
                c2 = ROR(c2, 1);
                byte[] c3 = text_branches[3];
                c3 = ROL(c3, 1);
                c3 = XOR(f_result[1], c3);

                text_branches = new byte[][] { c2, c3, text_branches[0], text_branches[1] };
            }

            //Выходное отбеливание
            byte[][] keys_output_whitening = new byte[][] { k_keys[4], k_keys[5], k_keys[6], k_keys[7] };
            Whitening(ref text_branches, ref keys_output_whitening);

            // Слияние 4 ветвей в конечный закрытый текст
            byte[] ciphertext = new byte[16];
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                    ciphertext[i * 4 + j] = text_branches[i][j];

            return ciphertext;
        }

        public byte[] Decrypt_block(byte[] ciphertext)
        {
            if (ciphertext.Length != 16)
                throw new Exception("Something go wrong! The block size is not equal to 128");
            if (key == null)
                throw new Exception("Key is null. Use SetKey method.");

            // Разделение закрытого текста на 4 ветви
            byte[][] text_branches = new byte[4][];
            for (int i = 0; i < 4; i++)
            {
                text_branches[i] = new byte[4];
                for (int j = 0; j < 4; j++)
                {
                    text_branches[i][j] = ciphertext[i * 4 + j];
                }
            }

            // Ключевое расписание
            KeyShedule(out k_keys, out s_keys, ref key);

            // Входное отбеливание
            byte[][] keys_input_whitening = new byte[][] { k_keys[4], k_keys[5], k_keys[6], k_keys[7] };
            Whitening(ref text_branches, ref keys_input_whitening);

            // 16 раундов сети Фейстеля
            for (int round = 15; round > -1; round--)
            {

                byte[][] f_result = f_function(text_branches[2], text_branches[3], round);

                byte[] c2 = ROL(text_branches[0], 1);
                c2 = XOR(f_result[0], c2);
                byte[] c3 = XOR(f_result[1], text_branches[1]);
                c3 = ROR(c3, 1);

                text_branches = new byte[][] { text_branches[2], text_branches[3], c2, c3 };
            }

            //Выходное отбеливание
            byte[][] keys_output_whitening = new byte[][] { k_keys[0], k_keys[1], k_keys[2], k_keys[3] };
            Whitening(ref text_branches, ref keys_output_whitening);

            // Слияние 4 ветвей в конечный закрытый текст
            byte[] plaintext = new byte[16];
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                    plaintext[i * 4 + j] = text_branches[i][j];

            return plaintext;

        }

        private void KeyShedule(out byte[][] k_keys, out byte[][] s_keys, ref byte[] main_key)
        {
            int k = main_key.Length / 8; // 2

            // Построение M_even и M_odd
            byte[] m_even = new byte[main_key.Length / 2];
            byte[] m_odd = new byte[main_key.Length / 2];

            for (int i = 0; i < k; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    m_even[i * 4 + j] = main_key[i * 8 + j];
                    m_even[i * 4 + j] = main_key[i * 8 + 4 + j];
                }
            }

            // Построение S ключей
            s_keys = new byte[k][];
            for (int i = 0; i < k; i++)
            {
                byte[,] tmp_matrix = new byte[8, 1];
                for (int j = 0; j < 8; j++)
                {
                    tmp_matrix[j, 0] = main_key[i * 8 + j];
                }

                byte[,] rez = MatrixMultiplication(RS, tmp_matrix, 0x4D); // x^8 + x^6 + x^3 + x^2 + 1

                s_keys[i] = new byte[4];
                for (int j = 0; j < 4; j++)
                {
                    s_keys[i][j] = rez[j, 0];
                }
            }

            // Построение K ключей            
            k_keys = h_function(m_even, m_odd);


        }

        private void Whitening(ref byte[][] text, ref byte[][] keys)
        {
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                    text[i][j] = (byte)(text[i][j] ^ keys[i][j]);
        }

        private byte[][] h_function(byte[] m_even, byte[] m_odd)
        {
            byte[] M0 = m_even.Take(4).ToArray();
            byte[] M1 = m_odd.Take(4).ToArray();
            byte[] M2 = m_even.Skip(4).ToArray();
            byte[] M3 = m_odd.Skip(4).ToArray();

            byte[][] k_keys = new byte[40][];

            for(byte i = 0; i < 40; i+=2)
            {
                byte[,] key_1_matrix = MatrixMultiplication(MDS, h_function_Sboxs(i,M2, M0), 0x69); // x^8 + x^6 + x^5 + x^3 + 1
                byte[,] key_2_matrix = MatrixMultiplication(MDS, h_function_Sboxs((byte)(i+1), M3, M1), 0x69); // x^8 + x^6 + x^5 + x^3 + 1 

                byte[] key_1 = { key_1_matrix[0, 0], key_1_matrix[1, 0], key_1_matrix[2, 0], key_1_matrix[3, 0] };
                byte[] key_2 = { key_2_matrix[0, 0], key_2_matrix[1, 0], key_2_matrix[2, 0], key_2_matrix[3, 0] };

                key_2 = ROL(key_2, 8);

                PHT(ref key_1, ref key_2);

                key_2 = ROL(key_2,9);

                k_keys[i] = key_1;
                k_keys[i+1] = key_2;
                
            }

            return k_keys;
        }

        private byte[,] h_function_Sboxs(byte input, byte[] M1, byte[] M2)
        {
            byte[,] output = new byte[4,1];

            output[0, 0] = q1((byte)(q0((byte)(q0(input) ^ M1[0])) ^ M2[0]));
            output[1, 0] = q0((byte)(q0((byte)(q1(input) ^ M1[1])) ^ M2[1]));
            output[2, 0] = q1((byte)(q1((byte)(q0(input) ^ M1[2])) ^ M2[2]));
            output[3, 0] = q0((byte)(q1((byte)(q1(input) ^ M1[3])) ^ M2[3]));

            return output;
        }

        private byte[][] f_function(byte[] R1, byte[] R2, int round)
        {
            R2 = ROL(R2,8);

            byte[] Z1 = g_function(R1);
            byte[] Z2 = g_function(R2);

            PHT(ref Z1, ref Z2);

            byte[] F1 = ADD(Z1, k_keys[round * 2 + 8]);
            byte[] F2 = ADD(Z2, k_keys[round * 2 + 9]);

            byte[][] result = new byte[][] {F1,F2};

            return result;
        }

        private byte[] g_function(byte[] R)
        {
            byte[,] Z_matrix = MatrixMultiplication(MDS, Sboxs(R), 0x69);  // x^8 + x^6 + x^5 + x^3 + 1
            byte[] Z = { Z_matrix[0, 0], Z_matrix[1, 0], Z_matrix[2, 0], Z_matrix[3, 0] };
            return Z;
        }

        private byte[,] Sboxs(byte[] input)
        {
            byte[,] output = new byte[4, 1];

            output[0, 0] = q1((byte)(q0((byte)(q0(input[0]) ^ s_keys[0][0])) ^ s_keys[1][0]));
            output[1, 0] = q0((byte)(q0((byte)(q1(input[1]) ^ s_keys[0][1])) ^ s_keys[1][1]));
            output[2, 0] = q1((byte)(q1((byte)(q0(input[2]) ^ s_keys[0][2])) ^ s_keys[1][2]));
            output[3, 0] = q0((byte)(q1((byte)(q1(input[3]) ^ s_keys[0][3])) ^ s_keys[1][3]));

            return output;
        }

        private byte q0(byte input)
        {
            byte a0 = (byte)(input & 15);
            byte b0 = (byte)(input / 16);

            byte a1 = (byte)(a0 ^ b0);
            byte b1 = (byte)(a0 ^ ROR(b0, 1, 4) ^ ((8 * a0) % 16));

            byte a2 = tq0[0, a1];
            byte b2 = tq0[1, b1];

            byte a3 = (byte)(a2 ^ b2);
            byte b3 = (byte)(a2 ^ ROR(b2, 1, 4) ^ ((8 * a2) % 16));

            byte a4 = tq0[2, a3];
            byte b4 = tq0[3, b3];

            return (byte)(16 * b4 + a4);
        }

        private byte q1(byte input)
        {
            byte a0 = (byte)(input & 15);
            byte b0 = (byte)(input / 16);

            byte a1 = (byte)(a0 ^ b0);
            byte b1 = (byte)(a0 ^ ROR(b0, 1, 4) ^ ((8 * a0) % 16));

            byte a2 = tq1[0, a1];
            byte b2 = tq1[1, b1];

            byte a3 = (byte)(a2 ^ b2);
            byte b3 = (byte)(a2 ^ ROR(b2, 1, 4) ^ ((8 * a2) % 16));

            byte a4 = tq1[2, a3];
            byte b4 = tq1[3, b3];

            return (byte)(16 * b4 + a4);
        }

        static private void PHT(ref byte[] T0, ref byte[] T1)
        {
            uint T0_uint = (uint)BytesToInt(T0);
            uint T1_uint = (uint)BytesToInt(T1);

            T0 = IntToBytes((int)(T0_uint + T1_uint));
            T1 = IntToBytes((int)(T0_uint + 2*T1_uint));
        }

        static private byte[,] MatrixMultiplication(byte[,] A, byte[,] B, byte module)
        {
            int rA = A.GetLength(0);  // 4
            int cA = A.GetLength(1);  // 8
            int rB = B.GetLength(0);  // 8
            int cB = B.GetLength(1);  // 1

            if (cA != rB)
            {
                throw new Exception("Matrixes can't be multiplied!!");
            }
            else
            {
                byte temp = 0;
                byte[,] kHasil = new byte[rA, cB];

                for (int i = 0; i < rA; i++)
                {
                    for (int j = 0; j < cB; j++)
                    {
                        temp = 0;
                        for (int k = 0; k < cA; k++)
                        {
                            temp ^= GaloisMultiplication(A[i, k], B[k, j], module);
                        }
                        kHasil[i, j] = temp;
                    }
                }

                return kHasil;
            }
        }
        static private byte GaloisMultiplication(byte a, byte b, byte module)  // Galois Field (256) Multiplication
        {
            byte result = 0;
            byte tmp;
            for (byte i = 0; i < 8; i++)
            {
                if ((b & 1) != 0)
                {
                    result ^= a;
                }
                tmp = (byte)(a & 0x80);
                a <<= 1;
                if (tmp != 0)
                {
                    a ^= module;
                }
                b >>= 1;
            }
            return result;
        }

        static private byte[] ROR(byte[] inp, int n)
        {
            uint inp_int = (uint)BytesToInt(inp);

            uint low = (uint)(inp_int & ((1 << n) - 1));
            uint high = inp_int >> n;

            uint result = high | (low << 32 - n);

            return IntToBytes((int)result);
        }

        static private byte ROR(byte inp, int n, int module)
        {
            byte low = (byte)(inp & ((1 << n) - 1));
            byte high = (byte)(inp >> n);

            byte result = (byte)(high | (low << module - n));

            return result;
        }

        static private byte[] ROL(byte[] inp, int n)
        {
            uint inp_int = (uint)BytesToInt(inp);

            uint low = (uint)(inp_int & ((1 << 32 - n) - 1));
            uint high = inp_int >> 32 - n;

            uint result = high | (low << n);

            return IntToBytes((int)result);
        }

        static private byte[] XOR(byte[] inp1, byte[] inp2)
        {
            if (inp1.Length != inp2.Length)
                throw new Exception("Something go wrong! For XORing length of inputs ,ust be equal");
            byte[] result = new byte[inp1.Length];
            for(int i = 0; i < inp1.Length; i++)
            {
                result[i] = (byte)(inp1[i] ^ inp2[i]);
            }

            return result;
        }

        static private byte[] ADD(byte[] inp1, byte[] inp2)
        {
            uint inp1_uint = (uint)BytesToInt(inp1);
            uint inp2_uint = (uint)BytesToInt(inp2);

            byte[] result = IntToBytes((int)(inp1_uint + inp1_uint));

            return result;
        }

        static private int BytesToInt(byte[] bytes) 
        {
            return (bytes[0] << 24) | (bytes[1] << 16) | (bytes[2] << 8) | bytes[3];
        }
        static private byte[] IntToBytes(int num) 
        {
            byte[] b = BitConverter.GetBytes(num);
            Array.Reverse(b);
            return b;
        }
    }

}
