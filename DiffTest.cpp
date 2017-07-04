/**
* @file MatchTemplate_Demo.cpp
* @brief Sample code to use the function MatchTemplate
* @author OpenCV team
*/

#include "stdafx.h"

using namespace std;
using namespace cv;

//! [declare]

const char* image_windowName = "Source Image";
const char* result_windowName = "Result window";

/// Function Headers
void MatchingMethod1(Mat orig , Mat diff);


Mat getGrayScale(Mat color) {	
	cv::Mat gs_rgb(color.size(), CV_8UC1);
	cv::cvtColor(color, gs_rgb, CV_RGB2GRAY);
	return gs_rgb;
}
/**
* @function main
*/
int main(int argc, char** argv)
{	

	//! [load_image]
	/// Load image and template
	Mat dOrigImg = getGrayScale(imread("data\\cctxt\\lib1.jpg", IMREAD_COLOR));
	Mat diffImage = getGrayScale(imread("data\\cctxt\\no.jpg", IMREAD_COLOR));

	if (dOrigImg.empty() || diffImage.empty() )
	{
		cout << "Can't read one of the images" << endl;
		return -1;
	}

	Mat diffresult;
	cv::absdiff(dOrigImg, diffImage, diffresult);

	Mat result;
	result.create(diffresult.size(), diffresult.type());
	for (int j = 0; j < diffImage.rows; ++j)
	{
		const uchar* current = diffImage.ptr<uchar>(j);

		uchar* output = result.ptr<uchar>(j);

		for (int i = 0; i < (diffImage.cols); i++)
		{
			*output = 0;
			if (current[i] > 200) {
				*output = 255;
			}
			output++;
		}
	}

	//! [load_image]

	//! [create_windows]
	/// Create windows
	namedWindow(image_windowName, WINDOW_NORMAL);
	namedWindow(result_windowName, WINDOW_NORMAL);

	imshow(image_windowName, diffImage);
	imshow(result_windowName, result);

	waitKey(0);
	return 0;
	//! [create_windows]

	//! [create_trackbar]
	/// Create Trackbar
	const char* trackbar_label = "Method: \n 0: SQDIFF \n 1: SQDIFF NORMED \n 2: TM CCORR \n 3: TM CCORR NORMED \n 4: TM COEFF \n 5: TM COEFF NORMED";
	
	//! [create_trackbar]

	MatchingMethod1(dOrigImg, diffImage);

	//! [wait_key]
	waitKey(0);
	return 0;
	//! [wait_key]
}


/**
* @function MatchingMethod
* @brief Trackbar callback
*/
void MatchingMethod1(Mat img, Mat templ)
{
	Mat img_display;
	img.copyTo(img_display);

	Mat result;
	double oldVal;
	Mat firstResult;
	int result_cols = img.cols - templ.cols + 1;
	int result_rows = img.rows - templ.rows + 1;

	result.create(result_rows, result_cols, CV_32FC1);
	//for (int loopi = 0; loopi < 4; loopi++) 
	{
		
			int match_method = CV_TM_SQDIFF;
			matchTemplate(img, templ, result, match_method);
		

		/// Localizing the best match with minMaxLoc
		double minVal; double maxVal; Point minLoc; Point maxLoc;
		Point matchLoc;

		minMaxLoc(result, &minVal, &maxVal, &minLoc, &maxLoc, Mat());
		//! [best_match]

		normalize(result, result, 0, 1, NORM_MINMAX, -1, Mat());
		
		//! [match_loc]
		/// For SQDIFF and SQDIFF_NORMED, the best matches are lower values. For all the other methods, the higher the better
		if (match_method == TM_SQDIFF || match_method == TM_SQDIFF_NORMED)
		{
			oldVal = minVal;
			matchLoc = minLoc;
		}
		else
		{
			oldVal = maxVal;
			matchLoc = maxLoc;
		}

		printf("val is %f\n", (float)oldVal);
		//! [match_loc]

		//! [imshow]
		/// Show me what you got
		rectangle(img_display, matchLoc, Point(matchLoc.x + templ.cols, matchLoc.y + templ.rows), Scalar::all(0), 2, 8, 0);
		rectangle(result, matchLoc, Point(matchLoc.x + templ.cols, matchLoc.y + templ.rows), Scalar::all(0), 2, 8, 0);

		imshow(image_windowName, img_display);
		imshow(result_windowName, result);

		//rectangle(img, matchLoc, Point(matchLoc.x + templ.cols, matchLoc.y + templ.rows), Scalar::all(0),-1);
		//! [imshow]
	}
	return;
}