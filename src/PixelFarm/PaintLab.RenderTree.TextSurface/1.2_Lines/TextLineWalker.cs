﻿//Apache2, 2014-present, WinterDev

using System;
using System.Collections.Generic;

using PixelFarm.CpuBlit;
using PixelFarm.Drawing;
using Typography.Text;
using Typography.TextLayout;

namespace LayoutFarm.TextEditing
{
    public readonly struct SelectionRangeInfo
    {
        public readonly EditableVisualPointInfo start;
        public readonly EditableVisualPointInfo end;
        public SelectionRangeInfo(EditableVisualPointInfo start, EditableVisualPointInfo end)
        {
            this.start = start;
            this.end = end;
        }
    }

    class TextFlowEditWalker : TextFlowWalkerBase
    {
        //TODO: review this class again 
        public TextFlowEditWalker(TextFlowLayer textLayer) : base(textLayer)
        {

        }



        public void Clear()
        {
            //clear all
            this.MoveToLine(0);
            ClearCurrentLine();
            EnsureCurrentTextRun();
        }

        public void EnsureCurrentTextRun(int index)
        {
            Run run = CurrentTextRun;
            if (run != null && run.HasParent)
            {
                SetCurrentCharIndex(index);
            }
            else
            {
                if (_needUpdateCurrentRun)
                {
                    SetCurrentCharIndex2(index);
                    _needUpdateCurrentRun = false;
                }
                else
                {
                    SetCurrentCharIndexToBegin();
                    if (index != -1)
                    {
                        int limit = CurrentLine.CharCount();
                        if (index > limit)
                        {
                            index = limit;
                        }
                        SetCurrentCharIndex(index);
                    }
                }

            }
        }

        public void EnsureCurrentTextRun() => EnsureCurrentTextRun(CharIndex);

        public void RemoveSelectedTextRuns(VisualSelectionRange selectionRange)
        {
#if DEBUG   
            //if (!CurrentLine.dbugHasOwner)
            //{

            //}
#endif
            int precutIndex = selectionRange.StartPoint.LineCharIndex;
            CurrentLine.Remove(selectionRange);
            InvalidateCurrentRun();

            CurrentLine.TextLineReCalculateActualLineSize();
            CurrentLine.RefreshInlineArrange();
            SetCurrentCharIndex2(precutIndex);
        }
        public void ClearCurrentLine()
        {
            int currentCharIndex = CharIndex;
            CurrentLine.ReplaceAll(null);
            CurrentLine.TextLineReCalculateActualLineSize();
            CurrentLine.RefreshInlineArrange();
            EnsureCurrentTextRun(currentCharIndex);
        }
        public void ReplaceCurrentLine(IEnumerable<Run> textRuns)
        {
            int currentCharIndex = CharIndex;
            CurrentLine.ReplaceAll(textRuns);
            CurrentLine.TextLineReCalculateActualLineSize();
            CurrentLine.RefreshInlineArrange();
            EnsureCurrentTextRun(currentCharIndex);
        }
        public void JoinWithNextLine()
        {
            TextLineBox.InnerDoJoinWithNextLine(this.CurrentLine);
            EnsureCurrentTextRun();
        }
        int BackSpaceOneChar()
        {
            if (CurrentTextRun == null)
            {
                return '\0';
            }
            else
            {
                if (CharIndex == 0)
                {
                    return '\0';
                }

                //move back 1 char and do delete 
                Run removingTextRun = CurrentTextRun;
                int removeIndex = CurrentTextRunCharIndex;
                SetCurrentCharStepLeft();
                int toBeRemovedChar = CurrentChar;
                int actualRemove = 1;

                //if (char.IsLowSurrogate(toBeRemovedChar))
                //{
                //    if (removeIndex > 0)
                //    {
                //        removeIndex--;
                //        actualRemove++;
                //    }
                //}

                if (removingTextRun is TextRun textRun)
                {

                    textRun.Remove(removeIndex, actualRemove, false);
                    //Run.InnerRemove(removingTextRun, removeIndex, actualRemove, false);

                    if (removingTextRun.CharacterCount == 0)
                    {
                        Run nextRun = removingTextRun.NextRun;
                        CurrentLine.Remove(removingTextRun);
                        SetCurrentTextRun(nextRun);
                        EnsureCurrentTextRun();

                    }
                    CurrentLine.TextLineReCalculateActualLineSize();
                    CurrentLine.RefreshInlineArrange();
                }

                return toBeRemovedChar;
            }
        }

        public Run GetCurrentTextRun() => CurrentLine.IsBlankLine ? null : CurrentTextRun;

        public bool CanAcceptThisChar(int c)
        {
            //TODO: review here, enable this feature or not
            //some char can't be a start char on blank line
            if (CurrentLine.IsBlankLine &&
                !TextFlowEditSession.CanCaretStopOnThisChar(c))
            {
                return false;
            }
            return true;
        }
        public void AddCharacter(int c)
        {
            if (CurrentLine.IsBlankLine)
            {
                //TODO: review here, enable this feature or not
                //some char can't be a start char on blank line

                if (!TextFlowEditSession.CanCaretStopOnThisChar(c))
                {
                    return;
                }
                //

                //1. new 
                TextRun run = CurrentLine.CreateTextRun(c);
                CurrentLine.AddLast(run);
                SetCurrentTextRun(run);
            }
            else
            {

                if (CurrentTextRun is TextRun run)
                {
                    if (run.IsInsertable)
                    {
                        run.InsertAfter(CurrentTextRunCharIndex, c);
                    }
                    else
                    {
                        AddTextSpan(CurrentLine.CreateTextRun(c));
                        return;
                    }
                }
                else
                {
                    throw new NotSupportedException();
                }
            }

            CurrentLine.TextLineReCalculateActualLineSize();
            CurrentLine.RefreshInlineArrange();
            SetCurrentCharStepRight();
        }
        public void AddTextSpan(string textspan)
        {
            AddTextSpan(CurrentLine.CreateTextRun(textspan));
        }
        public void AddTextSpan(char[] textspan)
        {
            AddTextSpan(CurrentLine.CreateTextRun(textspan));
        }
        public void AddTextSpan(ArraySegment<char> textspan)
        {
            AddTextSpan(CurrentLine.CreateTextRun(textspan));
        }
        public void AddTextSpan(ArraySegment<int> textspan)
        {
            AddTextSpan(CurrentLine.CreateTextRun(textspan));
        }
        public void AddTextSpan(Run textRun)
        {
            if (CurrentLine.IsBlankLine)
            {
                CurrentLine.AddLast(textRun);
                SetCurrentTextRun(textRun);
                CurrentLine.TextLineReCalculateActualLineSize();
                CurrentLine.RefreshInlineArrange();

                SetCurrentCharIndex(CharIndex + textRun.CharacterCount);
            }
            else
            {
                if (CurrentTextRun != null)
                {
                    VisualPointInfo newPointInfo = CurrentLine.Split(GetCurrentPointInfo());
                    if (newPointInfo.IsOnTheBeginOfLine)
                    {
                        if (newPointInfo.Run == null)
                        {
                            CurrentLine.AddFirst(textRun);
                        }
                        else
                        {
                            CurrentLine.AddBefore(newPointInfo.Run, textRun);
                        }
                    }
                    else
                    {
                        if (newPointInfo.Run == null)
                        {
                            CurrentLine.AddFirst(textRun);
                        }
                        else
                        {
                            CurrentLine.AddAfter(newPointInfo.Run, textRun);
                        }

                    }
                    CurrentLine.TextLineReCalculateActualLineSize();
                    CurrentLine.RefreshInlineArrange();
                    EnsureCurrentTextRun(CharIndex + textRun.CharacterCount);
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
        }
        public void ReplaceAllLineContent(Run[] runs)
        {
            int charIndex = CharIndex;
            CurrentLine.Clear();
            int j = runs.Length;
            for (int i = 0; i < j; ++i)
            {
                CurrentLine.AddLast(runs[i]);
            }

            EnsureCurrentTextRun(charIndex);
        }

        public int DoBackspaceOneChar() => BackSpaceOneChar();

        public int DoDeleteOneChar()
        {
            if (CharIndex < CurrentLine.CharCount())
            {
                //simulate backspace keystroke
                SetCurrentCharStepRight();
                return BackSpaceOneChar();
            }
            else
            {
                return '\0';
            }
        }

        public void SplitToNewLine()
        {
            Run currentRun = CurrentTextRun;
            if (CurrentLine.IsBlankLine)
            {
                CurrentLine.AddLineBreakAfterLastRun();
            }
            else
            {
                if (CharIndex == -1)
                {
                    CurrentLine.AddLineBreakBeforeFirstRun();
                    SetCurrentTextRun(null);
                }
                else
                {
                    if (currentRun is TextRun textrun)
                    {

                        CharSpan rightPart = textrun.Remove(CurrentTextRunCharIndex + 1, true);

                        if (rightPart.len > 0)
                        {
                            CurrentLine.AddAfter(currentRun, rightPart);
                        }

                        CurrentLine.AddLineBreakAfter(currentRun);
                        if (currentRun.CharacterCount == 0)
                        {
                            CurrentLine.Remove(currentRun);
                        }
                    }


                }
            }


            _textFlowLayer.TopDownReCalculateContentSize();
            EnsureCurrentTextRun();
        }
        public SelectionRangeInfo SplitSelectedText(VisualSelectionRange selectionRange, RunStyle runStyleForNewPart)
        {
            SelectionRangeInfo newPoints = CurrentLine.Split(selectionRange, runStyleForNewPart);
            EnsureCurrentTextRun();
            return newPoints;
        }
    }

    /// <summary>
    /// read only text flow walker
    /// </summary>
    abstract class TextFlowWalkerBase
    {

#if DEBUG
        static int dbugTotalId;
        int dbug_MyId;
        public debugActivityRecorder dbugTextManRecorder;
#endif

        private protected TextFlowLayer _textFlowLayer;
        TextLineBox _currentLine;
        int _currentLineY = 0;
        //Run _run1_x;
        Run _currentTextRun;
        //{
        //    get => _run1_x;
        //    set
        //    {
        //        _run1_x = value;
        //    }

        //}

        //int _caretXPos_1;
        int _caretXPos;
        //{
        //    get => _caretXPos_1;
        //    set
        //    {
        //        if (value != 0)
        //        {

        //        }
        //        _caretXPos_1 = value;
        //    }
        //}
        /// <summary>
        /// character offset of this run, start from start line, this value is reset for every current run
        /// </summary>
        int _rCharOffset = 0;
        /// <summary>
        /// pixel offset of this run, start from the begin of this line, this value is reset for every current run
        /// </summary>
        int _rPixelOffset = 0;
        public TextFlowWalkerBase(TextFlowLayer flowlayer)
        {

#if DEBUG
            this.dbug_MyId = dbugTotalId;
            dbugTotalId++;
#endif

            _textFlowLayer = flowlayer;
            //flowlayer.Reflow += flowlayer_Reflow;
            _currentLine = flowlayer.GetTextLine(0);
            if (_currentLine.FirstRun != null)
            {
                _currentTextRun = _currentLine.FirstRun;
            }
        }

        public RunStyle CurrentSpanStyle => _textFlowLayer.DefaultRunStyle;
#if DEBUG
        int _i_charIndex;
        int caret_char_index
        {
            get { return _i_charIndex; }
            set
            {
                if ((value % 2) != 0)
                {

                }
                _i_charIndex = value;
            }
        }
#else
         int caret_char_index;
#endif

        //void flowlayer_Reflow(object sender, EventArgs e)
        //{
        //    int prevCharIndex = caret_char_index;
        //    this.SetCurrentCharIndexToBegin();
        //    this.SetCurrentCharIndex(prevCharIndex);
        //}
        // 
        // 
        //
        protected TextLineBox CurrentLine => _currentLine;
        //
        protected Run CurrentTextRun => _currentTextRun;
        //
        protected void SetCurrentTextRun(Run r) => _currentTextRun = r;

        protected bool _needUpdateCurrentRun;
        public void InvalidateCurrentRun()
        {
            _currentTextRun = null;
            _needUpdateCurrentRun = true;
        }


        readonly LayoutWordVisitor _wordVisitor = new LayoutWordVisitor();
        readonly LineSegmentList<LineSegment> _lineSegs = new LineSegmentList<LineSegment>();
        readonly ArrayList<char> _utf16ArrList = new ArrayList<char>();
        readonly ArrayList<int> _utf32ArrList = new ArrayList<int>();

        public void FindCurrentHitWord(out int startAt, out int len)
        {
            if (_currentTextRun == null)
            {
                startAt = 0;
                len = 0;
                return;
            }

            using (new TextUtf32RangeCopyPoolContext<TextFlowWalkerBase>(out TextCopyBufferUtf32 output))
            {
                //copy to int32 arr list

                _currentLine.CopyLineContent(output);

                _lineSegs.Clear();
                _wordVisitor.SetLineSegmentList(_lineSegs);


                int content_len = output.Length;
                _utf16ArrList.Clear(content_len);
                _utf32ArrList.Clear(content_len);
                //output.CopyTo(_arrList.UnsafeInternalArray);
                _utf32ArrList.AdjustSize(content_len);

                output.CopyTo(_utf32ArrList);

                Typography.Text.GlobalTextService.TxtClient.BreakToLineSegments(
                    new Typography.Text.TextBufferSpan(_utf32ArrList.UnsafeInternalArray,
                    0, _utf32ArrList.Length),
                    _wordVisitor);

                if (_lineSegs.Count == 0)
                {
                    startAt = 0;
                    len = 0;
                    return;
                }

                int segcount = _lineSegs.Count;
                for (int i = 0; i < segcount; ++i)
                {
                    LineSegment seg = _lineSegs.GetLineSegment(i);
                    if (seg.StartAt + seg.Length >= caret_char_index)
                    {
                        //stop at this segment
                        startAt = seg.StartAt;
                        len = seg.Length;
                        return;
                    }
                }

            }


            //?
            startAt = 0;
            len = 0;
        }

        bool MoveToPreviousTextRun()
        {
            //#if DEBUG
            //            if (_currentTextRun.IsLineBreak)
            //            {
            //                throw new NotSupportedException();
            //            }
            //#endif
            if (_currentTextRun.PrevRun != null)
            {
                _currentTextRun = _currentTextRun.PrevRun;
                _rCharOffset -= _currentTextRun.CharacterCount;
                _rPixelOffset -= _currentTextRun.Width;
                caret_char_index = _rCharOffset + _currentTextRun.CharacterCount;
                _caretXPos = _rPixelOffset + _currentTextRun.Width;
                return true;
            }
            return false;
        }

        bool MoveToNextTextRun()
        {
            //#if DEBUG
            //            if (_currentTextRun.IsLineBreak)
            //            {
            //                throw new NotSupportedException();
            //            }
            //#endif


            Run nextTextRun = _currentTextRun.NextRun;
            if (nextTextRun != null)// && !nextTextRun.IsLineBreak)
            {
                _rCharOffset += _currentTextRun.CharacterCount;
                _rPixelOffset += _currentTextRun.Width;
                _currentTextRun = nextTextRun;
                caret_char_index = _rCharOffset;
                _caretXPos = _rPixelOffset + _currentTextRun.GetRunWidth(0);
                return true;
            }
            return false;
        }


        public void MoveToLine(int lineNumber)
        {
            TextLineBox linebox = _textFlowLayer.GetTextLine(lineNumber);
            if (linebox == null)
            {
                //error=> line not found
                return;
            }

            //move to specific line box
            _currentLine = linebox;
            _currentLineY = _currentLine.Top;

            //if current line is a blank line
            //not first run => currentTextRun= null 
            _currentTextRun = _currentLine.FirstRun;

            _rCharOffset = 0;
            _rPixelOffset = 0;

            caret_char_index = 0;
            _caretXPos = 0;
        }
        public void CopyContentToStrignBuilder(TextCopyBuffer output)
        {
            _textFlowLayer.CopyContent(output);
        }
        public int PrevChar
        {
            get
            {
                if (_currentTextRun != null)
                {

                    if (caret_char_index == 0 && CharCount == 0)
                    {
                        return '\0';
                    }
                    if (caret_char_index == _rCharOffset)
                    {
                        if (_currentTextRun.PrevRun != null)
                        {
                            return (_currentTextRun.PrevRun).GetChar(_currentTextRun.PrevRun.CharacterCount - 1);
                        }
                        else
                        {
                            return '\0';
                        }
                    }
                    else
                    {
                        return _currentTextRun.GetChar(caret_char_index - _rCharOffset);
                    }
                }
                else
                {
                    return '\0';
                }
            }
        }
        public int NextChar
        {
            get
            {
                if (_currentTextRun != null)
                {

                    if (caret_char_index == 0 && CharCount == 0)
                    {
                        return '\0';
                    }
                    if (caret_char_index == _rCharOffset + _currentTextRun.CharacterCount)
                    {
                        if (_currentTextRun.NextRun != null)
                        {
                            return (_currentTextRun.NextRun).GetChar(0);
                        }
                        else
                        {
                            return '\0';
                        }
                    }
                    else
                    {
                        return _currentTextRun.GetChar(caret_char_index - _rCharOffset);
                    }
                }
                else
                {
                    return '\0';
                }
            }
        }
        /// <summary>
        /// next char width in this line
        /// </summary>
        public int NextCharWidth
        {
            get
            {
                if (_currentTextRun != null)
                {
                    if (CharCount == 0)
                    {
                        //no text in this line
                        return 0;
                    }
                    if (caret_char_index == _rCharOffset + _currentTextRun.CharacterCount)
                    {
                        //-----
                        //this is on the last of current run
                        //if we have next run, just get run of next width
                        //-----
                        Run nextRun = _currentTextRun.NextRun;
                        if (nextRun != null)
                        {
                            return nextRun.GetRunWidth(0);
                        }
                        else
                        {
                            return 0;
                        }
                    }
                    else
                    {
                        //in some line

                        return _currentTextRun.GetRunWidth(caret_char_index - _rCharOffset + 1) -
                                    _currentTextRun.GetRunWidth(caret_char_index - _rCharOffset);
                    }
                }
                else
                {
                    return 0;
                }
            }
        }

        public EditableVisualPointInfo GetCurrentPointInfo()
        {
#if DEBUG
            if (_currentTextRun != null && !_currentTextRun.HasParent)
            {
                throw new NotSupportedException();
            }
            if (_currentTextRun == null)
            {
            }
#endif
            EditableVisualPointInfo textPointInfo =
                new EditableVisualPointInfo(_currentLine, caret_char_index, _currentTextRun);
            textPointInfo.SetAdditionVisualInfo(
                _rCharOffset, _caretXPos, _rPixelOffset);
            return textPointInfo;
        }

        public Point CaretPosition => new Point(_caretXPos, _currentLineY);

        public int CurrentChar
        {
            get
            {
                //1. blank line
                if (_currentTextRun == null)
                {
                    return '\0';
                }
                else
                {
                    //2.  
                    if (_currentTextRun.CharacterCount == caret_char_index - _rCharOffset)
                    {
                        //end of this run
                        return '\0';
                    }

                    return _currentTextRun.GetChar(caret_char_index - _rCharOffset);
                }
            }
        }
        //
        public int CurrentTextRunCharOffset => _rCharOffset;
        //
        public int CurrentTextRunPixelOffset => _rPixelOffset;
        //
        public int CurrentTextRunCharIndex => caret_char_index - _rCharOffset - 1;
        //
        /// <summary>
        /// try set caret x pos to nearest request value
        /// </summary>
        /// <param name="xpos"></param>
        public void TrySetCaretPos(int xpos, int ypos)
        {

            //--------

            _textFlowLayer.LatestHitRun = null;
            //--------
            if (_currentTextRun == null)
            {
                caret_char_index = 0;
                _caretXPos = 0;
                _rCharOffset = 0;
                _rPixelOffset = 0;
                return;
            }
            int pixDiff = xpos - _caretXPos;
            if (pixDiff > 0)
            {
                do
                {
                    int thisTextRunPixelLength = _currentTextRun.Width;
                    if (_rPixelOffset + thisTextRunPixelLength > xpos)
                    {
                        CharLocation foundLocation = Run.InnerGetCharacterFromPixelOffset(_currentTextRun, xpos - _rPixelOffset);
#if DEBUG
                        if ((foundLocation.RunCharIndex % 2) != 0)
                        {

                        }
#endif
                        _caretXPos = _rPixelOffset + foundLocation.pixelOffset;
                        caret_char_index = _rCharOffset + foundLocation.RunCharIndex;

                        //for solid text run
                        //we can send some event to it
                        _textFlowLayer.LatestHitRun = _currentTextRun;
                        //if (foundLocation.charIndex == -1)
                        //{
                        //    if (!(MoveToPreviousTextRun()))
                        //    {
                        //        caretXPos = 0;
                        //        caret_char_index = 0;
                        //    }
                        //}
                        //else
                        //{
                        //    caretXPos = rPixelOffset + foundLocation.pixelOffset;
                        //    caret_char_index = rCharOffset + foundLocation.charIndex;
                        //}
                        return;
                    }
                } while (MoveToNextTextRun());
                //to the last
                _caretXPos = _rPixelOffset + _currentTextRun.Width;
                caret_char_index = _rCharOffset + _currentTextRun.CharacterCount;
                return;
            }
            else if (pixDiff < 0)
            {
                do
                {
                    if (xpos >= _rPixelOffset)
                    {
                        CharLocation foundLocation = Run.InnerGetCharacterFromPixelOffset(_currentTextRun, xpos - _rPixelOffset);
                        _caretXPos = _rPixelOffset + foundLocation.pixelOffset;
                        caret_char_index = _rCharOffset + foundLocation.RunCharIndex;


                        //for solid text run
                        //we can send some event to it

                        _textFlowLayer.LatestHitRun = _currentTextRun;
                        //if (foundLocation.charIndex == -1)
                        //{
                        //    if (!MoveToPreviousTextRun())
                        //    {
                        //        caret_char_index = 0;
                        //        caretXPos = 0;
                        //    }
                        //}
                        //else
                        //{
                        //    caretXPos = rPixelOffset + foundLocation.pixelOffset;
                        //    caret_char_index = rCharOffset + foundLocation.RunCharIndex;
                        //}
                        return;
                    }
                } while (MoveToPreviousTextRun());//
                _caretXPos = 0;
                caret_char_index = 0;
                return;
            }
        }
        //
        public int CaretXPos => _caretXPos;
        //
        public int CharIndex => caret_char_index;
        //
        int InternalCharIndex => caret_char_index;
        //
        public void SetCurrentCharStepRight()
        {
            SetCurrentCharIndex(InternalCharIndex + 1);
            //check current char is surrogate or not
            //int c = CurrentChar;
            //#if DEBUG
            //            bool is_high_surrogate = char.IsHighSurrogate(c);
            //            bool is_low_surrogate = char.IsLowSurrogate(c);
            //#endif

            //            if (char.IsLowSurrogate(c))
            //            {
            //                //can't stop at this 
            //                SetCurrentCharStepRight();
            //            }
        }

        public void SetCurrentCharStepLeft() => SetCurrentCharIndex(InternalCharIndex - 1);

        public void SetCurrentCharIndexToEnd() => SetCurrentCharIndex(this.CharCount);

        public void SetCurrentCharIndexToBegin() => SetCurrentCharIndex(0);

        public void SetCurrentCharIndex2(int newCharIndexPointTo)
        {

#if DEBUG
            if (dbugTextManRecorder != null)
            {
                dbugTextManRecorder.WriteInfo("TextLineReader::CharIndex_set=" + newCharIndexPointTo);
                dbugTextManRecorder.BeginContext();
            }
#endif
            if (newCharIndexPointTo < 0 || newCharIndexPointTo > _currentLine.CharCount())
            {
                throw new NotSupportedException("index out of range");
            }


            if (newCharIndexPointTo == 0)
            {
                caret_char_index = 0;
                _caretXPos = 0;
                _rCharOffset = 0;
                _rPixelOffset = 0;
                _currentTextRun = _currentLine.FirstRun;
            }
            else
            {
                caret_char_index = 0;
                _caretXPos = 0;
                _rCharOffset = 0;
                _rPixelOffset = 0;
                _currentTextRun = _currentLine.FirstRun;

                do
                {
                    if (_rCharOffset + _currentTextRun.CharacterCount >= newCharIndexPointTo)
                    {
                        caret_char_index = newCharIndexPointTo;
                        _caretXPos = _rPixelOffset + _currentTextRun.GetRunWidth(caret_char_index - _rCharOffset);
#if DEBUG
                        if (dbugTextManRecorder != null)
                        {
                            dbugTextManRecorder.EndContext();
                        }
#endif

                        return;
                    }
                    //
                } while (MoveToNextTextRun());
                caret_char_index = _rCharOffset + _currentTextRun.CharacterCount;
                _caretXPos = _rPixelOffset + _currentTextRun.Width;
                return;
            }
#if DEBUG
            if (dbugTextManRecorder != null)
            {
                dbugTextManRecorder.EndContext();
            }
#endif

        }
        public void SetCurrentCharIndex(int newCharIndexPointTo)
        {

#if DEBUG
            if (dbugTextManRecorder != null)
            {
                dbugTextManRecorder.WriteInfo("TextLineReader::CharIndex_set=" + newCharIndexPointTo);
                dbugTextManRecorder.BeginContext();
            }
#endif
            if (newCharIndexPointTo < 0 || newCharIndexPointTo > _currentLine.CharCount())
            {
                System.Diagnostics.Debugger.Break();
                return;
                // throw new NotSupportedException("index out of range");
            }


            if (newCharIndexPointTo == 0)
            {
                caret_char_index = 0;
                _caretXPos = 0;
                _rCharOffset = 0;
                _rPixelOffset = 0;
                _currentTextRun = _currentLine.FirstRun;
            }
            else
            {
                int diff = newCharIndexPointTo - caret_char_index;
                switch (diff)
                {
                    case 0:
                        return;
                    default:
                        {
                            if (diff > 0)
                            {
                                do
                                {
                                    if (_rCharOffset + _currentTextRun.CharacterCount >= newCharIndexPointTo)
                                    {
                                        caret_char_index = newCharIndexPointTo;
                                        _caretXPos = _rPixelOffset + _currentTextRun.GetRunWidth(caret_char_index - _rCharOffset);
#if DEBUG
                                        if (dbugTextManRecorder != null)
                                        {
                                            dbugTextManRecorder.EndContext();
                                        }
#endif

                                        return;
                                    }
                                    //
                                } while (MoveToNextTextRun());
                                caret_char_index = _rCharOffset + _currentTextRun.CharacterCount;
                                _caretXPos = _rPixelOffset + _currentTextRun.Width;
                                return;
                            }
                            else
                            {
                                do
                                {
                                    if (_rCharOffset - 1 < newCharIndexPointTo)
                                    {
                                        caret_char_index = newCharIndexPointTo;
                                        _caretXPos = _rPixelOffset + _currentTextRun.GetRunWidth(caret_char_index - _rCharOffset);
#if DEBUG
                                        if (dbugTextManRecorder != null)
                                        {
                                            dbugTextManRecorder.EndContext();
                                        }
#endif
                                        return;
                                    }
                                } while (MoveToPreviousTextRun());
                                caret_char_index = 0;
                                _caretXPos = 0;
                            }
                        }
                        break;
                }
            }
#if DEBUG
            if (dbugTextManRecorder != null)
            {
                dbugTextManRecorder.EndContext();
            }
#endif

        }

        //
        public bool IsOnEndOfLine => caret_char_index == _currentLine.CharCount();
        //
        internal TextLineBox GetTextLine(int lineId) => _textFlowLayer.GetTextLine(lineId);
        //
        internal TextLineBox GetTextLineAtPos(int y) => _textFlowLayer.GetTextLineAtPos(y);
        //
        public int LineCount => _textFlowLayer.LineCount;
        //
        public bool HasNextLine => _currentLine.Next != null;
        //
        public bool HasPrevLine => _currentLine.Prev != null;
        //
        public bool IsOnStartOfLine => InternalCharIndex == 0;
        //
        public int CharCount => _currentLine.CharCount();
        //
        public void CopyLineContent(TextCopyBuffer output) => _currentLine.CopyLineContent(output);

        //
        public void CopySelectedTextRuns(VisualSelectionRange selectionRange, TextCopyBuffer output) => _currentLine.Copy(selectionRange, output);

        //
        public int LineNumber => _currentLine.LineNumber;
        //
        public void MoveToNextLine() => MoveToLine(_currentLine.LineNumber + 1);

        public void MoveToPrevLine() => MoveToLine(_currentLine.LineNumber - 1);

        //
        public Rectangle LineArea => _currentLine.ActualLineArea;

    }
}