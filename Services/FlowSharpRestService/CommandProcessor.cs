﻿/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System;
using System.Linq;
using System.Reflection;

using Clifton.Core.Utils;
using Clifton.Core.ExtensionMethods;
using Clifton.Core.Semantics;

using FlowSharpLib;
using FlowSharpServiceInterfaces;
using FlowSharpCodeServiceInterfaces;
using FlowSharpCodeShapeInterfaces;

namespace FlowSharpRestService
{
    public class CommandProcessor : IReceptor
    {
        // Ex: localhost:8001/flowsharp?cmd=CmdUpdateProperty&Name=btnTest&PropertyName=Text&Value=Foobar
        public void Process(ISemanticProcessor proc, IMembrane membrane, CmdUpdateProperty cmd)
        {
            BaseController controller = proc.ServiceManager.Get<IFlowSharpCanvasService>().ActiveController;
            var els = controller.Elements.Where(e => e.Name == cmd.Name);

            els.ForEach(el =>
            {
                PropertyInfo pi = el.GetType().GetProperty(cmd.PropertyName);
                object cval = Converter.Convert(cmd.Value, pi.PropertyType);

                el?.Canvas.Invoke(() =>
                {
                    pi.SetValue(el, cval);
                    controller.Redraw(el);
                });
            });
        }

        // Ex: localhost:8001:flowsharp?cmd=CmdShowShape&Name=btnTest
        public void Process(ISemanticProcessor proc, IMembrane membrane, CmdShowShape cmd)
        {
            BaseController controller = proc.ServiceManager.Get<IFlowSharpCanvasService>().ActiveController;
            var el = controller.Elements.Where(e => e.Name == cmd.Name).FirstOrDefault();

            el?.Canvas.Invoke(() =>
            {
                controller.FocusOn(el);
            });
        }

        // Ex: localhost:8001/flowsharp?cmd=CmdGetShapeFiles
        public void Process(ISemanticProcessor proc, IMembrane membrane, CmdGetShapeFiles cmd)
        {
            BaseController controller = proc.ServiceManager.Get<IFlowSharpCanvasService>().ActiveController;
            var els = controller.Elements.Where(e => e is IFileBox);
            cmd.Filenames.AddRange(els.Cast<IFileBox>().Where(el => !String.IsNullOrEmpty(el.Filename)).Select(el => el.Filename));
        }

        // FlowSharpCodeOutputWindowService required for this behavior.
        // Ex: localhost:8001/flowsharp?cmd=CmdOutputMessage&Text=foobar
        public void Process(ISemanticProcessor proc, IMembrane membrane, CmdOutputMessage cmd)
        {
            var w = proc.ServiceManager.Get<IFlowSharpCodeOutputWindowService>();
            cmd.Text.Split('\n').Where(s=>!String.IsNullOrEmpty(s.Trim())).ForEach(s => w.WriteLine(s.Trim()));
        }
    }
}