﻿using System.ComponentModel;
using Android.Content.Res;
using Android.Graphics;
using Android.Text;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Java.Lang;
using Android.Views;
using Android.Util;
using System;

[assembly: ExportRenderer(typeof(Forms9Patch.Label), typeof(Forms9Patch.Droid.LabelRenderer))]
namespace Forms9Patch.Droid
{
	/// <summary>
	/// The Forms9Patch Label renderer.
	/// </summary>
	public class LabelRenderer : ViewRenderer<Label, F9PTextView>
	{
		ColorStateList _labelTextColorDefault;
		int _lastHeightConstraint;
		int _lastWidthConstraint;
		int _lastLines;
		LabelFit _lastFit = LabelFit.None;

		SizeRequest? _lastSizeRequest;
		//float _lastTextSize = -1f;
		Typeface _lastTypeface;

		//Xamarin.Forms.Color _lastUpdateColor = Xamarin.Forms.Color.Default;
		//TextView _view;
		//F9PTextView _view;
		//bool _wasFormatted;

		/// <summary>
		/// Initializes a new instance of the <see cref="LabelRenderer"/> class.
		/// </summary>
		public LabelRenderer()
		{
			AutoPackage = false;
		}

		int _lastDesiredSizeWidthConstraint = -1;
		int _lastDesiredSizeHeightConstraint = -1;

		void LayoutForSize(int width, int height)
		{
			var widthConstraint = MeasureSpec.MakeMeasureSpec(width, MeasureSpecMode.AtMost);
			var heightConstraint = MeasureSpec.MakeMeasureSpec(height, MeasureSpecMode.AtMost);
			GetDesiredSize(widthConstraint,heightConstraint);
		}

		/// <summary>
		/// Gets the size of the desired.
		/// </summary>
		/// <returns>The desired size.</returns>
		/// <param name="widthConstraint">Width constraint.</param>
		/// <param name="heightConstraint">Height constraint.</param>
		public override SizeRequest GetDesiredSize(int widthConstraint, int heightConstraint)
		{
			//string text = Element.Text ?? Element.FormattedText?.Text;
			//Element.Sized = true;
			if (string.IsNullOrEmpty(Element.Text ?? Element.F9PFormattedString?.Text) || Control == null)
			{
				_lastSizeRequest = null;
				return new SizeRequest(Xamarin.Forms.Size.Zero);
			}

			ICharSequence text;
			if (Element.Text != null)
				text = new Java.Lang.String(Element.Text);
			else
			{
				if (Settings.IsLicenseValid || Element._id < 4)
					text = Element.F9PFormattedString.ToSpannableString();
				else
					text = new Java.Lang.String("UNLICENSED COPY");
			}


			int availWidth = MeasureSpec.GetSize(widthConstraint);
			if (MeasureSpec.GetMode(widthConstraint) == Android.Views.MeasureSpecMode.Unspecified)
				availWidth = int.MaxValue;
			int availHeight = MeasureSpec.GetSize(heightConstraint);
			if (MeasureSpec.GetMode(heightConstraint) == Android.Views.MeasureSpecMode.Unspecified)
				availHeight = int.MaxValue;

			//System.Diagnostics.Debug.WriteLine("[" + (Element.HtmlText ?? Element.Text) + "]LabelRenderer.GetDesiredSize(" + availWidth + "," + availHeight + ")");
			_lastDesiredSizeWidthConstraint = availWidth;
			_lastDesiredSizeHeightConstraint = availHeight;


			if (availWidth <= 0 || availHeight <= 0)
			{
				_lastSizeRequest = null;
				return new SizeRequest(Xamarin.Forms.Size.Zero);
			}

			//System.Diagnostics.Debug.WriteLine("LabelRenderer.GetDesiredSize(" + availWidth + "," + availHeight + ") enter text=[" + text + "]");

			if (_lastSizeRequest.HasValue)
			{
				bool canRecycleLast = availWidth == _lastWidthConstraint && availHeight == _lastHeightConstraint;
				canRecycleLast = canRecycleLast && _lastLines == Element.Lines && _lastFit == Element.Fit;
				if (canRecycleLast)
				{
					//System.Diagnostics.Debug.WriteLine("\tLabelRenderer.GetDesiredSize\trecycled["+_lastSizeRequest.Value+"]");
					return _lastSizeRequest.Value;
				}
			}


			var tmpFontSize = ModelFontSize;
			Control.TextSize = tmpFontSize;
			Control.SetSingleLine(false);
			Control.SetMaxLines(int.MaxValue);
			Control.SetIncludeFontPadding(false);
			Control.Ellipsize = null;

			int tmpHt = -1;
			int tmpWd = -1;

			var fontMetrics = Control.Paint.GetFontMetrics();
			var fontLineHeight = fontMetrics.Descent - fontMetrics.Ascent;
			var fontLeading = System.Math.Abs(fontMetrics.Bottom - fontMetrics.Descent);

			if (Element.Lines == 0 && Element.Fit != LabelFit.None)
			{
				tmpFontSize = F9PTextView.ZeroLinesFit(text, new TextPaint(Control.Paint), ModelMinFontSize, tmpFontSize, availWidth, availHeight);
			}
			else if (Element.Fit == LabelFit.Lines)
			{

				if (int.MaxValue == availHeight)
				{
					// we need to set the height of the Control to be Lines * FontHeight
					//tmpHt = Control.Font.LineHeight * Element.Lines + Control.Font.Leading * (Element.Lines - 1);
					tmpHt = (int)System.Math.Round(Element.Lines * fontLineHeight + (Element.Lines - 1) * fontLeading);
				}
				else
				{
					var fontPointSize = tmpFontSize;
					var lineHeightRatio = fontLineHeight / fontPointSize;
					var leadingRatio = fontLeading / fontPointSize;

					tmpFontSize = ((availHeight / (Element.Lines + leadingRatio * (Element.Lines - 1))) / lineHeightRatio - 0.1f);
					/*
					Control.LinesToFit = -1;
					double error;
					do
					{
						error = Control.LinesFitError(text,tmpFontSize, widthConstraint, heightConstraint);
						if (error < -0.0005)
							tmpFontSize /= 1.05f;
					}
					while (error < -0.0005);
					*/
				}
			}
			else if (Element.Fit == LabelFit.Width)
				tmpFontSize = F9PTextView.WidthFit(text, new TextPaint(Control.Paint), Element.Lines, ModelMinFontSize, tmpFontSize, availWidth, availHeight);
			//else
			//	Control.SetMaxLines(Element.Lines);

			Control.TextSize = BoundTextSize(tmpFontSize);
			//System.Diagnostics.Debug.WriteLine("\tLabelRenderer.GetDesiredSize\tControl.TextSize=["+Control.TextSize+"]");

			//Control.SetBackgroundColor(Android.Graphics.Color.Orange);

			var layout = new StaticLayout(text, new TextPaint(Control.Paint), availWidth, Android.Text.Layout.Alignment.AlignNormal, 1.0f, 0.0f, true);
			//System.Diagnostics.Debug.WriteLine("\tLabelRenderer.GetDesiredSize\tpost-fit layout.size=[" + layout.Width + ", "+layout.Height +"]");
			//System.Diagnostics.Debug.WriteLine("\tLabelRenderer.GetDesiredSize\ttmp.size=[" + tmpWd + ", " + tmpHt+ "]");
			//int lines = Element.Fit==LabelFit.None ? layout.LineCount : Element.Lines;
			int lines = Element.Lines;
			if (lines == 0 && Element.Fit==LabelFit.None)
			{
				for (int i = 0; i < layout.LineCount; i++)
				{
					if (layout.GetLineBottom(i) <= availHeight - layout.TopPadding - layout.BottomPadding)
					{
						//System.Diagnostics.Debug.WriteLine("layout.GetLineBottom("+i+")=["+layout.GetLineBottom(i)+"]");
						lines++;
					}
					else
						break;
				}
			}
			//System.Diagnostics.Debug.WriteLine("availHeight=["+(availHeight - layout.TopPadding - layout.BottomPadding)+"]");
			//System.Diagnostics.Debug.WriteLine("lines=["+lines+"]");
			//System.Diagnostics.Debug.WriteLine("layout Height=["+layout.Height+"] lineCount=["+layout.LineCount+"]");

			if (layout.Height > availHeight || (lines > 0 && layout.LineCount > lines))
			{
				if (Element.Lines == 1)
				{
					Control.SetSingleLine(true);
					Control.SetMaxLines(1);
					Control.Ellipsize = Element.LineBreakMode.ToEllipsize();
				}
				else
				layout = F9PTextView.Truncate(Element.Text, Element.F9PFormattedString, new TextPaint(Control.Paint), (int)(availWidth), availHeight, Element.Fit, Element.LineBreakMode, ref lines, ref text);
				//System.Diagnostics.Debug.WriteLine("\tLabelRenderer.GetDesiredSize\tlayout.LineCount=[" + layout.LineCount + "]");
				//System.Diagnostics.Debug.WriteLine("\tLabelRenderer.GetDesiredSize\tpost-truncate layout.size=[" + layout.Width + ", " + layout.Height + "]");
			}
			lines = lines > 0 ? System.Math.Min(lines, layout.LineCount) : layout.LineCount;
			for (int i = 0; i < lines; i++)
			{
				tmpHt = layout.GetLineBottom(i);
				var width = layout.GetLineWidth(i);
				//System.Diagnostics.Debug.WriteLine("\t\tright=["+right+"]");
				if (width > tmpWd)
					tmpWd = (int)System.Math.Ceiling(width);
			}
			if (Element.Fit == LabelFit.None && Element.Lines > 0)
			{
				Control.SetMaxLines(Element.Lines);
			}

			//System.Diagnostics.Debug.WriteLine("\tLabelRenderer.GetDesiredSize\ttmp.size=[" + tmpWd + ", " + tmpHt + "]");
			if (Element.IsDynamicallySized && Element.Lines > 0 && Element.Fit == LabelFit.Lines)
			{
				fontMetrics = Control.Paint.GetFontMetrics();
				fontLineHeight = fontMetrics.Descent - fontMetrics.Ascent;
				fontLeading = System.Math.Abs(fontMetrics.Bottom - fontMetrics.Descent);
				tmpHt = (int)(fontLineHeight * Element.Lines + fontLeading * (Element.Lines - 1));
			}
			//System.Diagnostics.Debug.WriteLine("\tLabelRenderer.GetDesiredSize\ttmp.size=[" + tmpWd + ", " + tmpHt + "]");

			Control.Gravity = Element.HorizontalTextAlignment.ToHorizontalGravityFlags() | Element.VerticalTextAlignment.ToVerticalGravityFlags();
			//ViewGroup.SetForegroundGravity(Element.HorizontalTextAlignment.ToHorizontalGravityFlags() | Element.VerticalTextAlignment.ToVerticalGravityFlags());
			//ViewGroup.SetBackgroundColor(Android.Graphics.Color.Orchid);
			//ViewGroup.SetForegroundGravity(Control.Gravity);

			if (Element.Text != null)
				Control.Text = text.ToString();
			else
				Control.TextFormatted = text;

			_lastWidthConstraint = availWidth;
			_lastHeightConstraint = availHeight;
			_lastLines = Element.Lines;
			_lastFit = Element.Fit;
			_lastSizeRequest = new SizeRequest(new Xamarin.Forms.Size(tmpWd, tmpHt), new Xamarin.Forms.Size(10, tmpHt));
			if (!_delayingActualFontSizeUpdate)
			{
				_delayingActualFontSizeUpdate = true;
				Device.StartTimer(TimeSpan.FromMilliseconds(100), () =>
				{
					_delayingActualFontSizeUpdate = false;
					Element.ActualFontSize = Control.TextSize;
					return false;
				});
			}
			return _lastSizeRequest.Value;
		}

		bool _delayingActualFontSizeUpdate;

		/// <summary>
		/// Raises the element changed event.
		/// </summary>
		/// <param name="e">E.</param>
		protected override void OnElementChanged(ElementChangedEventArgs<Label> e)
		{
			base.OnElementChanged(e);
			if (Control == null)
			{
				//_view = new TextView(Context);
				var view = new F9PTextView(Context);
				_labelTextColorDefault = view.TextColors;
				SetNativeControl(view);
			}


			if (e.OldElement != e.NewElement && e.NewElement != null)
			{
				Initialize();
			}


			if (e.OldElement == null)
			{
				UpdateText();
			}
			else
			{
				Control.SkipNextInvalidate();
				UpdateText();
			}
		}


		void Initialize()
		{
			//Control.IsDynamicallySized = () => Element.IsDynamicallySized;
			UpdateFont();
		}


		protected override void OnLayout(bool changed, int l, int t, int r, int b)
		{
			base.OnLayout(changed, l, t, r, b);
		}



		/// <summary>
		/// Raises the element property changed event.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">E.</param>
		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == Label.HorizontalTextAlignmentProperty.PropertyName || e.PropertyName == Label.VerticalTextAlignmentProperty.PropertyName)
			{
				Control.Gravity = Element.HorizontalTextAlignment.ToHorizontalGravityFlags() | Element.VerticalTextAlignment.ToVerticalGravityFlags();
				//_lastSizeRequest = null;
			}
			else if (e.PropertyName == Label.TextColorProperty.PropertyName)
				UpdateText();
			else if (e.PropertyName == Label.FontProperty.PropertyName)
				UpdateText();
			else if (e.PropertyName == Label.LineBreakModeProperty.PropertyName)
				_lastSizeRequest = null;
			else if (e.PropertyName == Label.TextProperty.PropertyName || e.PropertyName == Label.HtmlTextProperty.PropertyName)
				UpdateText();
			else if (e.PropertyName == Label.HeightProperty.PropertyName || e.PropertyName == Label.WidthProperty.PropertyName)
			{

				if (Element.Width > -1 && Element.Height > -1 && Element.IsVisible)
					if (Element.Width != _lastDesiredSizeWidthConstraint || Element.Height != _lastDesiredSizeHeightConstraint)
					{
					LayoutForSize((int)(Element.Width * Forms9Patch.Display.Scale), (int)(Element.Height * Forms9Patch.Display.Scale));
					}
						//Invalidate();
						//((Forms9Patch.Label)Element).InternalInvalidateMeasure();

			}
		}


		void UpdateColor()
		{
			Xamarin.Forms.Color c = Element.TextColor;
			//if (c == _lastUpdateColor)
			//	return;
			//_lastUpdateColor = c;
			if (c.ToAndroid () == Control.CurrentTextColor)
				return;

			//if (c.IsDefault)
			if (c==Xamarin.Forms.Color.Default)
				Control.SetTextColor(_labelTextColorDefault);
			else
				Control.SetTextColor(c.ToAndroid());
		}

		void UpdateFont(bool noDelay = false)
		{
			//System.Diagnostics.Debug.WriteLine("LabelRenderer.UpdateFont enter");
			Font f = Element.Font;

			Typeface newTypeface = FontManagment.TypefaceForFontFamily(Element.FontFamily);
			if (newTypeface == null)
				newTypeface = f.ToTypeface();
			/*
			Typeface newTypeface;
			if (Element.FontFamily!=null && Element.FontFamily.Contains (".Resources.Fonts.")) {
				newTypeface = FontManagment.TypefaceForFontFamily (Element.FontFamily);
			} else {
				newTypeface = f.ToTypeface ();
			}
			*/

			if (newTypeface != _lastTypeface)
			{
				Control.Typeface = newTypeface;
				_lastTypeface = newTypeface;
			}


			/*
			//float newTextSize = f.ToScaledPixel();
			//var density = Forms.Context.Resources.DisplayMetrics.Density;
			//System.Diagnostics.Debug.WriteLine("f.FontSize=["+f.FontSize+"]");
			float newTextSize = (float)f.FontSize;
			if (newTextSize == 0)
				newTextSize = F9PTextView.DefaultTextSize;
			if (newTextSize < 0)
				newTextSize = (float)(F9PTextView.DefaultTextSize * System.Math.Abs(f.FontSize));
			if (System.Math.Abs(newTextSize - Control.TextSize) > double.Epsilon * 5)
			//if (System.Math.Abs(newTextSize - _lastTextSize) > double.Epsilon*5)
			{
				//_view.SetTextSize(ComplexUnitType.Sp, newTextSize);
				if (noDelay)
					Control.PrivateSetTextSize(newTextSize);
				else
					Control.TextSize = newTextSize;
				//_lastTextSize = newTextSize;
			}
			//System.Diagnostics.Debug.WriteLine("\tLabelRenderer.UpdateFont exit");
			*/
		}

		float ModelFontSize
		{ 
			get
			{
				var textSize = (float)Element.Font.FontSize;
				if (System.Math.Abs(textSize) < 0.0001)
					textSize = F9PTextView.DefaultTextSize;
				//System.Diagnostics.Debug.WriteLine("ModelFontSize=["+textSize+"]");
				return BoundTextSize(textSize);
			}
		}

		float BoundTextSize(float textSize)
		{
			if (textSize < 0)
				textSize = (float)(F9PTextView.DefaultTextSize * System.Math.Abs(Element.Font.FontSize));
			if (textSize > ModelMaxFontSize)
				textSize = ModelMaxFontSize;
			if (textSize < ModelMinFontSize)
				textSize = ModelMinFontSize;
			return textSize;
		}

		float ModelMinFontSize
		{
			get
			{
				var minFontSize = (float)Element.MinFontSize;
				if (minFontSize < 0)
					minFontSize = 4;
				return minFontSize;
			}
		}

		float ModelMaxFontSize
		{
			get
			{
				var maxFontSize = (float)Element.MaxFontSize;
				if (maxFontSize < 0)
					maxFontSize = 256;
				return maxFontSize;
			}
		}

		void UpdateText()
		{

			/*
			//System.Diagnostics.Debug.WriteLine("LabelRenderer.UpdateText() enter");
			if (Element.HtmlText != null)
			{
				//FormattedString formattedText = Element.FormattedText ?? Element.Text;
				//_view.TextFormatted = formattedText.ToAttributed(Element.Font, Element.TextColor, _view);

				//_view.TextFormatted = Element.ToSpannableString ();
				if (Settings.IsLicenseValid || Element._id < 4)
					//_view.TextFormatted = Element.FormattedText.ToSpannableString();
					//Control.BaseFormattedString = Element.FormattedText;
					Control.TextFormatted = Element.FormattedText.ToSpannableString();
				else {
					Control.Text = "UNLICENSED COPY";
					Element.Text = Control.Text;
				}

				////System.Diagnostics.Debug.WriteLine ("HTML=["+Element.HtmlText+"]");
				////System.Diagnostics.Debug.WriteLine ("\tTFmt= ["+_view.TextFormatted+"]");
			}
			else
			{
				if (_wasFormatted)
				{
					Control.SetTextColor(_labelTextColorDefault);
					//_lastUpdateColor = Xamarin.Forms.Color.Default;
				}
				Control.Text = Element.Text;
				//_view.SetText(Element.Text, TextView.BufferType.Spannable);
				////System.Diagnostics.Debug.WriteLine ("TEXT=["+Element.Text+"]");
				////System.Diagnostics.Debug.WriteLine ("\tText= ["+_view.Text+"]");
			}
			*/

			if (Element.F9PFormattedString != null)
			{
				if (Settings.IsLicenseValid || Element._id < 4)
					Control.TextFormatted = Element.F9PFormattedString.ToSpannableString();
				else
					Control.Text = "UNLICENSED COPY";
			}
			else
				Control.Text = Element.Text;

			UpdateColor();
			UpdateFont ();
			//_wasFormatted = true;
			_lastSizeRequest = null;
			//System.Diagnostics.Debug.WriteLine("\tLabelRenderer.UpdateText() exit");
			////System.Diagnostics.Debug.WriteLine ("\tFrame=["+_view.Width+", "+_view.Height+"]");
		}
	}
}