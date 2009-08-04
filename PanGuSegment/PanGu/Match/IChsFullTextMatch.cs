using System;
using System.Collections.Generic;
using System.Text;

namespace PanGu.Match
{

    /// <summary>
    /// interface for Chinese full text match 
    /// </summary>
    public interface IChsFullTextMatch
    {
        MatchOptions Options { get; set; }

        /// <summary>
        /// Do match
        /// </summary>
        /// <param name="positionLenArr">array of position length</param>
        /// <param name="count">count of items of position length list</param>
        /// <returns>Word Info list</returns>
        LinkedList<WordInfo> Match(Dict.PositionLength[] positionLenArr, int stringLength, int count);
        
    }
}
