using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleAppTwofish
{
    public class AlgorithmTwofish
    {
        private byte[,] RS = new byte[,]
                                        {{0x01, 0xA4, 0x55, 0x87, 0x5A, 0x58, 0xDB, 0x9E},
                                        {0xA4, 0x56, 0x82, 0xF3, 0x1E, 0xC6, 0x68, 0xE5},
                                        {0x02, 0xA1, 0xFC, 0xC1, 0x47, 0xAE, 0x3D, 0x19},
                                        {0xA4, 0x55, 0x87, 0x5A, 0x58, 0xDB, 0x9E, 0x03}};
        private byte[,] tq0 = new byte[,]
                                        {{0x8, 0x1, 0x7, 0xD, 0x6, 0xF, 0x3, 0x2, 0x0, 0xB, 0x5, 0x9, 0xE, 0xC, 0xA, 0x4},
                                        {0xE, 0xC, 0xB, 0x8, 0x1, 0x2, 0x3, 0x5, 0xF, 0x4, 0xA, 0x6, 0x7, 0x0, 0x9, 0xD},
                                        {0xB, 0xA, 0x5, 0xE, 0x6, 0xD, 0x9, 0x0, 0xC, 0x8, 0xF, 0x3, 0x2, 0x4, 0x7, 0x1},
                                        {0xD, 0x7, 0xF, 0x4, 0x1, 0x2, 0x6, 0xE, 0x9, 0xB, 0x3, 0x0, 0x8, 0x5, 0xC, 0xA}};
        private byte[,] tq1 = new byte[,]
                                        {{0x2, 0x8, 0xB, 0xD, 0xF, 0x7, 0x6, 0xE, 0x3, 0x1, 0x9, 0x4, 0x0, 0xA, 0xC, 0x5},
                                        {0x1, 0xE, 0x2, 0xB, 0x4, 0xC, 0x3, 0x7, 0x6, 0xD, 0xA, 0x5, 0xF, 0x9, 0x0, 0x8},
                                        {0x4, 0xC, 0x7, 0x5, 0x1, 0x6, 0x9, 0xA, 0x0, 0xE, 0xD, 0x8, 0x2, 0xB, 0x3, 0xF},
                                        {0xB, 0x9, 0x5, 0x1, 0xC, 0x3, 0xD, 0xE, 0x6, 0x4, 0x7, 0xF, 0x2, 0x0, 0x8, 0xA}};

        private byte[,] MDS = new byte[,]
                                        {{0x01, 0xEF, 0x5B, 0x5B},
                                        {0x5B, 0xEF, 0xEF, 0x01},
                                        {0xEF, 0x5B, 0x01, 0xEF},
                                        {0xEF, 0x01, 0xEF, 0x5B}};
        private int key_length;
        private byte[] key;

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

        public byte[] Encrypt_block(byte[] plaintext)
        {
            if (plaintext.Length != 16)
                throw new Exception("Something go wrong! The block size is not equal to 128");
            if (key == null)
                throw new Exception("Key is null. Use SetKey method.");

            // Разделение открытого текста на 4 ветви
            byte[,] plaintext_branches = new byte[4, 4];
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    plaintext_branches[i, j] = plaintext[i + j];
                }
            }

            // Ключевое расписание
            byte[,] k_keys;
            byte[,] s_keys;
            KeyShedule(out k_keys, out s_keys, ref key);

            // Входное отбеливание
            Whitening(ref plaintext_branches, ref k_keys);

            // 16 раундов сети Фейстеля
            for (int round = 0; round < 16; round++)
            {

            }

            //Выходное отбеливание
            Whitening(ref plaintext_branches, ref k_keys);

            // Слияние 4 ветвей в конечный закрытый текст
            byte[] ciphertext = new byte[16];
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                    ciphertext[i + j] = plaintext_branches[i, j];

            return ciphertext;
        }
        public byte[] Decrypt_block()
        {
            return new byte[43];

        }

        private void KeyShedule(out byte[,] k_keys, out byte[,] s_keys, ref byte[] main_key)
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
            s_keys = new byte[k, 4];
            for (int i = 0; i < k; i++)
            {
                byte[,] tmp_matrix = new byte[8, 1];
                for (int j = 0; j < 8; j++)
                {
                    tmp_matrix[j, 0] = main_key[i * 8 + j];
                }

                byte[,] rez = MatrixMultiplication(RS, tmp_matrix);

                for (int j = 0; j < 4; j++)
                {
                    s_keys[i, j] = rez[0, j];
                }
            }

            // Построение K ключей            
            k_keys = h_function(m_even, m_odd);


        }

        private void Whitening(ref byte[,] plaintext, ref byte[,] keys)
        {
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                    plaintext[i, j] = (byte)(plaintext[i, j] ^ keys[i, j]);
        }

        private byte[,] h_function(byte[] m_even, byte[] m_odd)
        {
            byte[] M0 = m_even.Take(4).ToArray();
            byte[] M1 = m_odd.Take(4).ToArray();
            byte[] M2 = m_even.Skip(4).ToArray();
            byte[] M3 = m_odd.Skip(4).ToArray();

            byte[,] k_keys = new byte[40, 4];


        }

        private byte[,] MatrixMultiplication(byte[,] A, byte[,] B)
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
                            temp ^= GaloisMultiplication(A[i, k], B[k, j]);
                        }
                        kHasil[i, j] = temp;
                    }
                }

                return kHasil;
            }
        }
        private byte GaloisMultiplication(byte a, byte b)  // Galois Field (256) Multiplication
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
                    a ^= 0x4D; /* x^8 + x^4 + x^3 + x + 1 */
                }
                b >>= 1;
            }
            return result;
        }

        //def gf2n_multiply(a, b, modulus):
        //    overflow = 0x100
        //    sum1 = 0
        //    while (b > 0):
        //        if (b & 1):
        //            sum1 = sum1 ^ a
        //        b = b >> 1
        //        a = a << 1
        //        if (a & overflow):
        //            a = a ^ modulus
        //    return sum1

        //static private int BytesToInt(byte[] bytes) //Hex string to Byte[] converter
        //{
        //    return (bytes[0] << 24) | (bytes[1] << 16) | (bytes[2] << 8) | bytes[3];
        //}

        //static private byte[] HexStringToByteArray(String hex) //Hex string to Byte[] converter
        //{
        //    int NumberChars = hex.Length;
        //    byte[] bytes = new byte[NumberChars / 2];
        //    for (int i = 0; i < NumberChars; i += 2)
        //        bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
        //    return bytes;
        //}
    }
}
