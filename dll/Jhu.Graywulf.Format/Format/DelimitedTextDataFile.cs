﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;
using System.Globalization;
using System.Xml;

namespace Jhu.Graywulf.Format
{
    [Serializable]
    public class DelimitedTextDataFile : TextDataFileBase, IDisposable
    {
        [NonSerialized]
        private bool isFirstBlock;

        private char comment;
        private char quote;
        private char separator;

        #region Properties

        public override FileFormatDescription Description
        {
            get
            {
                return new FileFormatDescription()
                    {
                        DisplayName = FileFormatNames.Jhu_Graywulf_Format_DelimitedTextDataFile,
                        DefaultExtension = Constants.FileExtensionCsv,
                        CanRead = true,
                        CanWrite = true,
                        CanDetectColumnNames = true,
                        CanHoldMultipleDatasets = false,
                        IsCompressed = false,
                    };
            }
        }

        public char Comment
        {
            get { return comment; }
            set { comment = value; }
        }

        public char Quote
        {
            get { return quote; }
            set { quote = value; }
        }

        public char Separator
        {
            get { return separator; }
            set { separator = value; }
        }

        #endregion
        #region Constructors and initializers

        protected DelimitedTextDataFile()
            : base()
        {
            InitializeMembers();
        }

        public DelimitedTextDataFile(Uri uri, DataFileMode fileMode, Encoding encoding, CultureInfo culture)
            : base(uri, fileMode, encoding, culture)
        {
            InitializeMembers();

            Open();
        }

        public DelimitedTextDataFile(Uri uri, DataFileMode fileMode)
            : this(uri, fileMode, Encoding.ASCII, CultureInfo.InvariantCulture)
        {
            // Overload
        }

        public DelimitedTextDataFile(Stream stream, DataFileMode fileMode, Encoding encoding, CultureInfo culture)
            : base(stream, fileMode, encoding, culture)
        {
            InitializeMembers();

            Open();
        }

        public DelimitedTextDataFile(Stream stream, DataFileMode fileMode)
            : this(stream, fileMode, Encoding.ASCII, CultureInfo.InvariantCulture)
        {
            // Overload
        }

        public DelimitedTextDataFile(TextReader inputReader, Encoding encoding, CultureInfo culture)
            : base(inputReader, encoding, culture)
        {
            InitializeMembers();
        }

        public DelimitedTextDataFile(TextReader inputReader)
            : this(inputReader, Encoding.ASCII, CultureInfo.InvariantCulture)
        {
            // Overload
        }

        public DelimitedTextDataFile(TextWriter outputWriter, Encoding encoding, CultureInfo culture)
            : base(outputWriter, encoding, culture)
        {
            InitializeMembers();
        }

        public DelimitedTextDataFile(TextWriter outputWriter)
            : this(outputWriter, Encoding.ASCII, CultureInfo.InvariantCulture)
        {
            // Overload
        }

        private void InitializeMembers()
        {
            this.isFirstBlock = true;

            this.comment = '#';
            this.quote = '"';
            this.separator = ',';
        }

        #endregion
        #region Stream open and close

        public override void Open(TextReader inputReader)
        {
            base.Open(inputReader);

            Open();
        }

        public override void Open(TextWriter outputWriter)
        {
            base.Open(outputWriter);

            Open();
        }

        #endregion
        #region DataFileBase overloads

        /// <summary>
        /// Advanced the file to the next block.
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        protected override DataFileBlockBase OnReadNextBlock(DataFileBlockBase block)
        {
            // Only allow calling this function once
            if (!isFirstBlock)
            {
                return null;
            }

            isFirstBlock = false;

            // Create a new block object, if necessary
            return block ?? new DelimitedTextDataFileBlock(this);
        }

        protected internal override void OnReadFooter()
        {
            // No footer in csv files
        }

        /// <summary>
        /// Initializes writing the next block.
        /// </summary>
        /// <param name="block"></param>
        /// <param name="dr"></param>
        /// <returns></returns>
        /// If next block
        protected override DataFileBlockBase OnWriteNextBlock(DataFileBlockBase block, IDataReader dr)
        {
            if (!IsArchive && !isFirstBlock)
            {
                throw new InvalidOperationException();
                // CSV files can contain a single file block only
            }

            return block ?? new DelimitedTextDataFileBlock(this);
        }

        #endregion
    }
}