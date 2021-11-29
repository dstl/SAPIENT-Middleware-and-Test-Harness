// Project:           $Project: 00010855-Sapient$
// File:              $Workfile: IGUIInterface.cs$
// <copyright file="IGUIInterface.cs" company="QinetiQ">
// Crown-Owned Copyright (c) supplied by QinetiQ. See Release/Supply Conditions
// </copyright>

namespace SapientASMsimulator
{
    /// <summary>
    /// Interface to user interface
    /// </summary>
    public interface IGUIInterface
    {
        /// <summary>
        /// method to Update Output Window that can be called from outside the UI thread
        /// </summary>
        /// <param name="message">message to update with</param>
        void UpdateOutputWindow(string message);

        /// <summary>
        /// Update ASM ID text
        /// </summary>
        /// <param name="text">ASM ID text</param>
        void UpdateASMText(string text);
    }
}
