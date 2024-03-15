﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileRenamer.Model {
    public class Audiobook : IComparable {

        public Audiobook( string title ) : this( ) {
            if( !title.Contains( " - " ) ) {
                this.Title = title;
            } else {
                this.Title = title.Split( '-' )[1].Trim( );
                var tokens = title.Split( '-' )[0].Trim( ).Split( ' ' );
                // last token
                decimal num1;
                if( decimal.TryParse( tokens[tokens.Length - 1], out num1 ) ) {
                    //last token is numeric 
                    if( num1 < 100 ) {
                        // than 100: it's a number
                        this.Number = num1;
                    } else {
                        // greater than 100: a year
                        this.YearSeries = num1.ToString( "0.#" );
                    }
                }
                // remaining tokens
                if( string.IsNullOrEmpty( this.YearSeries ) ) {
                    var sb = new StringBuilder( );
                    foreach( var token in tokens ) {
                        decimal num2;
                        if( !Decimal.TryParse( token, out num2 ) ) {
                            sb.Append( string.Format( "{0} ", token ) );
                        }
                    }
                    this.YearSeries = sb.ToString( ).Trim( );
                }
            }
        }

        public Audiobook( ) {
            this.AuthorId = -1;
            this.AudiobookId = -1;
            this.Number = new Decimal( 0.0 );
            this.YearSeries = string.Empty;
        }
        public int AuthorId { get; set; }
        public int AudiobookId { get; set; }
        public string Title { get; set; }
        public string YearSeries { get; set; }
        public Decimal Number { get; set; }
                public DateTime CreatedDate { get; set; }

        public override string ToString( ) {
            return string.Format( "AudiobookId: {0}\nAuthorId: {1}\nTitle: {2}\nYearSeries: {3}\nNumber: {4}", this.AudiobookId, this.AuthorId, this.Title, this.YearSeries, this.Number );
        }

        public int CompareTo( object obj ) {
            var audiobook = (Audiobook)obj;
            return this.Title.CompareTo( audiobook.Title);
        }


    }
}